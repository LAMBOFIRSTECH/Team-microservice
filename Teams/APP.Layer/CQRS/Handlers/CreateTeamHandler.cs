using AutoMapper;
using MediatR;
using Teams.API.Layer.DTOs;
using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.CQRS.Commands;
using Teams.APP.Layer.Helpers;
using Teams.CORE.Layer.BusinessExceptions;
using Teams.CORE.Layer.Entities;
using Teams.CORE.Layer.Interfaces;

namespace Teams.APP.Layer.CQRS.Handlers;

public class CreateTeamHandler(
    ITeamRepository teamRepository,
    IMapper mapper,
    ILogger<CreateTeamHandler> log
) : IRequestHandler<CreateTeamCommand, TeamDto>
{
    public async Task<TeamDto> Handle(
        CreateTeamCommand command,
        CancellationToken cancellationToken
    )
    {
        var existingTeams = await teamRepository.GetAllTeamsAsync();
        if (existingTeams.Any(t => t.Name.Equals(command.Name, StringComparison.OrdinalIgnoreCase)))
        {
            LogHelper.BusinessRuleFailure(
                log,
                "Team creation",
                $"🚫 A team with the name '{command.Name}' already exists.",
                null
            );
            throw new DomainException($"A team with the name '{command.Name}' already exists.");
        }
        if (existingTeams.Count(t => t.TeamManagerId == command.TeamManagerId) > 3)
        {
            LogHelper.BusinessRuleFailure(
                log,
                "Team creation",
                "🚫 A manager cannot manage more than 3 teams.",
                null
            );
            throw new DomainException("A manager cannot manage more than 3 teams.");
        }

        if (
            existingTeams.Any(t =>
                t.MembersIds.Count == command.MembersId.Count
                && !t.MembersIds.Except(command.MembersId).Any()
                && t.TeamManagerId == command.TeamManagerId
            )
        )
        {
            LogHelper.BusinessRuleFailure(
                log,
                "Team creation",
                "🚫 A team with exactly the same members and manager already exists.",
                null
            );
            throw new DomainException(
                "A team with exactly the same members and manager already exists."
            );
        }
        var maxCommonPercent = GetCommonMembersStats(command.MembersId, existingTeams);
        if (maxCommonPercent >= 50)
        {
            LogHelper.BusinessRuleFailure(
                log,
                "Team creation",
                "🚫 Cannot create a team with more than 50% common members with existing teams.",
                null
            );
            throw new DomainException(
                "Cannot create a team with more than 50% common members with existing teams."
            );
        }
        Team team;
        try
        {
            team = Team.Create(command.Name!, command.TeamManagerId, command.MembersId, null);
        }
        catch (DomainException ex)
        {
            LogHelper.BusinessRuleFailure(log, "Team creation ", $"{ex.Message}", null);
            throw new HandlerException(400, ex.Message, "Bad Request", "Domain Validation Error");
        }
        await teamRepository.CreateTeamAsync(team);
        LogHelper.Info($"✅ Team {team.Name} has been created successfully.", log);
        return mapper.Map<TeamDto>(team);
    }

    public static double GetCommonMembersStats(List<Guid> newTeamMembers, List<Team> existingTeams)
    {
        if (newTeamMembers == null || newTeamMembers.Count == 0)
            throw new DomainException("The new team must have at least one member.");

        if (existingTeams == null || existingTeams.Count == 0)
            return 0; // Pas d'équipes existantes → pas de comparaison

        double maxPercent = 0;

        foreach (var existingTeam in existingTeams)
        {
            var common = existingTeam.MembersIds.Intersect(newTeamMembers).Count();
            var universe = existingTeam.MembersIds.Union(newTeamMembers).Count();
            double percent = (double)common / universe * 100;

            if (percent > maxPercent)
                maxPercent = percent;
        }

        return maxPercent;
    }
}
