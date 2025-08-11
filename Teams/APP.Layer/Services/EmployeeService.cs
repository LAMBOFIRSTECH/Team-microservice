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
                "Not allowed to be affected in a new team.",
                transfertMemberDto.MemberTeamId,
                "The team member {MemberTeamId} is not allowed to be affected in a new team."
            );
            throw DomainExceptionFactory.BusinessRule(
                $"The team member {transfertMemberDto.MemberTeamId} is not allowed to be affected in a new team.",
                "Not allowed member"
            );
        }

        if (DateTime.UtcNow < transfertMemberDto.AffectationStatus.LeaveDate.AddDays(7))
        {
            LogHelper.BusinessRuleViolated(
                "Member Cooldown Period",
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
                "No new member data could be retrieved from external service, fallback applied.",
                log
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
                $"Mismatch between member ID Rabbit : {memberId} | Employees Management microservice : {transfertMemberDto.MemberTeamId} "
            );
        }
        var team = await teamRepository.GetTeamByNameAsync(transfertMemberDto.DestinationTeam);
        if (team == null)
        {
            LogHelper.Error(
                $"Cannot found team {transfertMemberDto.DestinationTeam} in database.",
                log
            );
            throw DomainExceptionFactory.NotFound(
                transfertMemberDto.DestinationTeam,
                transfertMemberDto.MemberTeamId
            );
        }

        var result = CanMemberJoinNewTeam(team, transfertMemberDto);
        if (!result)
        {
            LogHelper.BusinessRuleViolated(
                "Cannot found team to add new member",
                transfertMemberDto.MemberTeamId,
                $"Team: {transfertMemberDto.DestinationTeam}"
            );
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
        LogHelper.Info(
            $"Team member {transfertMemberDto.MemberTeamId} has been store correctly in redis cache database",
            log
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
        {
            LogHelper.Error(
                $"Team '{teamName}' not found. Please check the team name or ensure the team exists.",
                log
            );
            throw new DomainException($"Team '{teamName}' not found.");
        }
        switch (action)
        {
            case TeamMemberAction.Add:
                if (team.MembersIds != null && team.MembersIds.Contains(memberId))
                {
                    LogHelper.BusinessRuleViolated(
                        "Member already exists in the team",
                        memberId,
                        $"Team: {teamName}"
                    );
                    throw new DomainException(
                        $"Member '{memberId}' already exists in team '{teamName}'."
                    );
                }
                team.AddMember(memberId);
                break;

            case TeamMemberAction.Remove:
                if (team.MembersIds == null || !team.MembersIds.Contains(memberId))
                {
                    LogHelper.BusinessRuleViolated(
                        "Member does not exist in the team",
                        memberId,
                        $"Team: {teamName}"
                    );
                    throw new DomainException(
                        $"Member '{memberId}' does not exist in team '{teamName}'."
                    );
                }
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
            LogHelper.Error($"Cannot found team member {memberId} in team {teamName}.", log);
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

    public async Task<bool> InsertNewTeamMemberIntoDbAsync(Guid memberId)
    {
        var teamName = await redisCache.GetNewTeamMemberFromCacheAsync(memberId);
        var teamMember = await teamRepository.GetTeamByNameAsync(teamName);
        if (teamMember == null)
        {
            LogHelper.Error(
                $"Cannot found team {teamName} in database for member {memberId}.",
                log
            );
            return false;
        }
        try
        {
            await ManageTeamMemberAsync(memberId, teamName, TeamMemberAction.Add, teamMember);
            await teamRepository.SaveAsync();
            return true;
        }
        catch (DomainException ex)
        {
            throw HandlerException.NotFound(ex.Message, "Domain validation failed");
        }
    }
    // Contrat des membres
    // Planning des membres
}
