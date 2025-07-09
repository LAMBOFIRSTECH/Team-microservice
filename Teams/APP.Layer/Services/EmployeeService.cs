using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.ExternalServicesDtos;
using Teams.APP.Layer.Interfaces;
using Teams.CORE.Layer.BusinessExceptions;
using Teams.CORE.Layer.Entities;
using Teams.CORE.Layer.Interfaces;
using Teams.CORE.Layer.Models;
using Teams.CORE.Layer.ValueObjects;
using Teams.INFRA.Layer.ExternalServices;

namespace Teams.APP.Layer.Services;

public class EmployeeService(
    ITeamRepository teamRepository,
    ILogger<EmployeeService> log,
    TeamExternalService teamExternalService,
    IRedisCacheService redisCache
) : IEmployeeService
{
    public (bool, Message?) CanMemberJoinNewTeam(Team team, TransfertMemberDto transfertMemberDto)
    {
        if (team.MembersIds.Count == 0)
            return (true, null); // Le membre n'existe pas dans une équipe
        if (!transfertMemberDto.AffectationStatus.IsTransferAllowed)
            return (
                false,
                new Message
                {
                    Status = 400,
                    Detail =
                        $"The team member {transfertMemberDto.MemberTeamId} cannot be added in a new team.",
                    Type = "Business Rule Violation",
                    Title = "Not allow member",
                }
            );
        if (transfertMemberDto.AffectationStatus.LeaveDate.AddDays(7) > DateTime.UtcNow)
            return (
                false,
                new Message
                {
                    Type = "Business Rule Violation",
                    Title = "Member Cooldown Period",
                    Detail =
                        $"member {transfertMemberDto!.MemberTeamId} must wait 7 days before being added to a new team.",
                    Status = 400,
                }
            ); // Moins de 7 jours : refus
        return (true, null);
    }

    public async Task AddTeamMemberAsync(Guid memberId)
    {
        var transfertMemberDto = await teamExternalService.RetrieveNewMemberToAddAsync();

        if (transfertMemberDto == null)
            throw new DomainException(
                "Business Rule Violation",
                "Missing Member Data",
                "No new member data could be retrieved.",
                "Received null from RetrieveNewMemberToAddAsync."
            );
        if (
            !transfertMemberDto.MemberTeamId.Equals(memberId)
            || transfertMemberDto.MemberTeamId == Guid.Empty
        )
            throw new DomainException(
                "Business Rule Violation",
                "Member ID Mismatch",
                $"The provided member ID {memberId} does not match the new member's ID {transfertMemberDto.MemberTeamId}.",
                $"Expected: {transfertMemberDto.MemberTeamId}, Provided: {memberId}"
            );

        var team = await teamRepository.GetTeamByNameAsync(transfertMemberDto.DestinationTeam);
        if (team == null)
            throw new DomainException(
                "Internal Server Error",
                "Team Not Found",
                $"No team found with Name {transfertMemberDto.DestinationTeam}.",
                $"Requested Team Name: {transfertMemberDto.DestinationTeam}"
            );

        CanMemberJoinNewTeam(team, transfertMemberDto);
        ManageTeamMemberAsync(
            transfertMemberDto.MemberTeamId,
            transfertMemberDto.DestinationTeam,
            TeamMemberAction.Add,
            team
        );
        redisCache.StoreNewTeamMemberInformationsInRedis(
            transfertMemberDto.MemberTeamId,
            transfertMemberDto.DestinationTeam
        );
        await teamRepository.AddTeamMemberAsync(); // C'est dans le cas redis la décision de rajouter un new member dépend du Manager (Authorized) dans le controller
        // filtrer les logs de hangfire pour n'afficher que les errors
    }

    public void ManageTeamMemberAsync(
        Guid memberId,
        string teamName,
        TeamMemberAction action,
        Team team
    )
    {
        if (team == null)
            throw new DomainException($"Team '{teamName}' not found.");
        switch (action)
        {
            case TeamMemberAction.Add:
                if (team.MembersIds != null && team.MembersIds.Contains(memberId))
                    throw new DomainException(
                        $"Member '{memberId}' already exists in team '{teamName}'."
                    );
                team.AddMember(memberId);
                break;

            case TeamMemberAction.Remove:
                if (team.MembersIds == null || !team.MembersIds.Contains(memberId))
                    throw new DomainException(
                        $"Member '{memberId}' does not exist in team '{teamName}'."
                    );
                team.DeleteTeamMemberSafely(memberId);
                teamRepository.SaveAsync();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }

    public async Task DeleteTeamMemberAsync(Guid memberId, string teamName)
    {
        var teamMember = await teamRepository.GetTeamByNameAndMemberIdAsync(memberId, teamName)!;
        if (teamMember == null)
            throw new DomainException(
                $"A team with the name '{teamName}' not found.",
                "Team Name not found",
                "No team found with the provided name.",
                $"Requested Team Name: {teamName}"
            );
        try
        {
            ManageTeamMemberAsync(memberId, teamName, TeamMemberAction.Remove, teamMember);
            await teamRepository.DeleteTeamMemberAsync();
        }
        catch (DomainException ex)
        {
            throw HandlerException.BadRequest(ex.Message, "Domain validation failed");
        }
    }
}
