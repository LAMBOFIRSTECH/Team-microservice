using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.Exceptions;
using Teams.APP.Layer.Helpers;
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
    public bool CanMemberJoinNewTeam(Team team, TransfertMemberDto transfertMemberDto)
    {
        if (team.MembersIds.Count == 0)
            return false;

        if (!transfertMemberDto.AffectationStatus.IsTransferAllowed)
        {
            LogHelper.BusinessRuleViolated(
                log,
                "The team member {MemberTeamId} is not allowed to be affected in a new team.",
                transfertMemberDto.MemberTeamId
            );
            throw DomainExceptionFactory.BusinessRule(
                $"The team member {transfertMemberDto.MemberTeamId} is not allowed to be affected in a new team.",
                "Not allowed member"
            );
        }

        if (DateTime.UtcNow < transfertMemberDto.AffectationStatus.LeaveDate.AddDays(7))
        {
            LogHelper.BusinessRuleViolated(
                log,
                "Cooldown",
                transfertMemberDto.MemberTeamId,
                "Still in wait period"
            );
            throw DomainExceptionFactory.BusinessRule(
                $"member {transfertMemberDto.MemberTeamId} must wait 7 days before being added to a new team.",
                "Member Cooldown Period"
            );
        }
        return true;
    }

    public async Task AddTeamMemberIntoRedisCacheAsync(Guid memberId)
    {
        var transfertMemberDto = await teamExternalService.RetrieveNewMemberToAddInRedisAsync();

        if (transfertMemberDto is null)
        {
            LogHelper.Info(
                log,
                "No new member data could be retrieved from external service, fallback applied."
            );
            throw ServicesExceptionFactory.Unavailable(
                "Employees Management microservice",
                "Missing Member Data"
            );
        }
        if (
            !transfertMemberDto.MemberTeamId.Equals(memberId)
            || transfertMemberDto.MemberTeamId == Guid.Empty
        )
        {
            LogHelper.CriticalFailure(
                log,
                "Teams.APP.Layer.Services EmployeeService",
                $"RabbitMq Message show {memberId} whereas Employees Management microservice sented {transfertMemberDto.MemberTeamId}."
            );
            throw DomainExceptionFactory.Conflict(
                transfertMemberDto.MemberTeamId.ToString(),
                $"Mismatch between member IDs Rabbit : {memberId} | Employees Management microservice : {transfertMemberDto.MemberTeamId} "
            );
        }
        var team = await teamRepository.GetTeamByNameAsync(transfertMemberDto.DestinationTeam);
        if (team == null)
        {
            log.LogError(
                "Cannot found team in database {DestinationTeam}",
                transfertMemberDto.DestinationTeam
            );
            throw DomainExceptionFactory.NotFound(
                transfertMemberDto.DestinationTeam,
                transfertMemberDto.MemberTeamId
            );
        }

        var result = CanMemberJoinNewTeam(team, transfertMemberDto);
        if (!result)
        {
            log.LogError("Cannot found team to add new member");
            throw new DomainException(
                404,
                "Cannot found team to add new member",
                null,
                "Business Rule Violation"
            );
        }
        try
        {
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
        catch (DomainException ex)
        {
            throw HandlerException.NotFound(ex.Message, "Domain validation failed");
        }
        log.LogInformation("Team member has been store correctly in redis cache database");
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
        {
            log.LogError("Impossible to delete team member in {TeamName}", teamName);
            throw DomainExceptionFactory.NotFound(teamName, memberId);
        }
        try
        {
            await ManageTeamMemberAsync(memberId, teamName, TeamMemberAction.Remove, teamMember);
            await teamRepository.DeleteTeamMemberAsync();
        }
        catch (DomainException ex)
        {
            throw HandlerException.NotFound(ex.Message, "Domain validation failed");
        }
    }

    public async Task InsertNewTeamMemberIntoDbAsync(Guid memberId)
    {
        var teamName = await redisCache.GetNewTeamMemberFromCacheAsync(memberId);
        var teamMember = await teamRepository.GetTeamByNameAsync(teamName);
        if (teamMember == null)
        {
            log.LogError("Impossible to insert team member in {TeamName}", teamName);
            throw DomainExceptionFactory.NotFound(teamName, memberId);
        }
        try
        {
            await ManageTeamMemberAsync(memberId, teamName, TeamMemberAction.Add, teamMember);
            await teamRepository.SaveAsync();
        }
        catch (DomainException ex)
        {
            throw HandlerException.NotFound(ex.Message, "Domain validation failed");
        }
    }
}
