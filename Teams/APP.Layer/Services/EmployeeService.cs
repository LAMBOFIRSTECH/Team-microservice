using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.Interfaces;
using Teams.CORE.Layer.BusinessExceptions;
using Teams.CORE.Layer.Entities;
using Teams.CORE.Layer.Interfaces;
using Teams.CORE.Layer.Models;
using Teams.INFRA.Layer.ExternalServices;
using Teams.INFRA.Layer.ExternalServicesDtos;

namespace Teams.APP.Layer.Services;

public class EmployeeService(
    ITeamRepository teamRepository,
    ILogger<EmployeeService> log,
    TeamExternalService teamExternalService,
    IRedisCacheService redisCache
) : IEmployeeService
{
    public Message? CanMemberJoinNewTeam(Team team, TransfertMemberDto transfertMemberDto)
    {
        if (team.MembersIds.Count == 0)
            return null;

        if (!transfertMemberDto.AffectationStatus.IsTransferAllowed)
        {
            return new Message(
                400,
                "Not allowed member",
                $"The team member {transfertMemberDto.MemberTeamId} cannot be added to a new team.",
                "Business Rule Violation"
            );
        }

        if (DateTime.UtcNow < transfertMemberDto.AffectationStatus.LeaveDate.AddDays(7))
        {
            return new Message(
                400,
                "Member Cooldown Period",
                $"member {transfertMemberDto.MemberTeamId} must wait 7 days before being added to a new team.",
                "Business Rule Violation"
            );
        }

        return null;
    }

    public async Task AddTeamMemberIntoRedisCacheAsync(Guid memberId)
    {
        var transfertMemberDto = await teamExternalService.RetrieveNewMemberToAddInRedisAsync();

        if (transfertMemberDto is null)
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

        var errors = CanMemberJoinNewTeam(team, transfertMemberDto);
        if (errors != null)
        {
            throw new DomainException(
                errors.Type,
                errors.Title,
                errors.Detail,
                errors.Status.ToString()
            );
        }

        await ManageTeamMemberAsync(
            transfertMemberDto.MemberTeamId,
            transfertMemberDto.DestinationTeam,
            TeamMemberAction.Add,
            team
        );
        await redisCache.StoreNewTeamMemberInformationsInRedisAsync(
            transfertMemberDto.MemberTeamId,
            transfertMemberDto.DestinationTeam
        );
    }

    public async Task ManageTeamMemberAsync(
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
                await teamRepository.SaveAsync();
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
            await ManageTeamMemberAsync(memberId, teamName, TeamMemberAction.Remove, teamMember);
            await teamRepository.DeleteTeamMemberAsync();
        }
        catch (DomainException ex)
        {
            throw HandlerException.BadRequest(ex.Message, "Domain validation failed");
        }
    }

    public async Task InsertNewTeamMemberIntoDbAsync(Guid memberId)
    {
        var teamName = await redisCache.GetNewTeamMemberFromCacheAsync(memberId);
        var teamMember = await teamRepository.GetTeamByNameAsync(teamName);
        if (teamMember == null)
            throw new DomainException(
                $"A team with the name '{teamName}' not found.",
                "Team Name not found",
                "No team found with the provided name.",
                $"Requested Team Name: {teamName}"
            );
        await ManageTeamMemberAsync(memberId, teamName, TeamMemberAction.Add, teamMember);
        await teamRepository.SaveAsync();
    }
}
