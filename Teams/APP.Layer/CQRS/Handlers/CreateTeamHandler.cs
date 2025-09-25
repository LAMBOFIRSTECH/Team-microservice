using AutoMapper;
using MediatR;
using Teams.API.Layer.DTOs;
using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.CQRS.Commands;
using Teams.APP.Layer.EventNotification;
using Teams.APP.Layer.Helpers;
using Teams.CORE.Layer.BusinessExceptions;
using Teams.CORE.Layer.CoreEvents;
using Teams.CORE.Layer.Entities;
using Teams.CORE.Layer.Interfaces;

namespace Teams.APP.Layer.CQRS.Handlers;
public class CreateTeamHandler(
    ITeamRepository teamRepository,
    IMapper mapper,
    ILogger<CreateTeamHandler> log,
    IMediator _mediator
) : IRequestHandler<CreateTeamCommand, TeamDto>
{
    private const string value = "Team creation";

    public async Task<TeamDto> Handle(
        CreateTeamCommand command,
        CancellationToken cancellationToken
    )
    {
        var existingTeams = await teamRepository.GetAllTeamsAsync(cancellationToken);
        if (existingTeams.Any(t => t.Name.Value.Equals(command.Name, StringComparison.OrdinalIgnoreCase)))
        {
            LogHelper.BusinessRuleFailure(
                log,
                value,
                $"🚫 A team with the name '{command.Name}' already exists.",
                null
            );
            throw new DomainException($"A team with the name '{command.Name}' already exists.");
        }
        if (existingTeams.Count(t => t.TeamManagerId.Value == command.TeamManagerId) > 3)
        {
            LogHelper.BusinessRuleFailure(
                log,
                value,
                "🚫 A manager cannot manage more than 3 teams.",
                null
            );
            throw new DomainException("A manager cannot manage more than 3 teams.");
        }

        if (
            existingTeams.Any(t =>
                t.MembersIds.Count == command.MembersIds.Count()
                && !t.MembersIds.Select(m => m.Value).Except(command.MembersIds).Any()
                && t.TeamManagerId.Value == command.TeamManagerId
            )
        )
        {
            LogHelper.BusinessRuleFailure(
                log,
                value,
                "🚫 A team with exactly the same members and manager already exists.",
                null
            );
            throw new DomainException(
                "A team with exactly the same members and manager already exists."
            );
        }
        var maxCommonPercent = GetCommonMembersStats(command.MembersIds, existingTeams);
        if (maxCommonPercent >= 50)
        {
            LogHelper.BusinessRuleFailure(
                log,
                value,
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
            team = Team.Create(command.Name!, command.TeamManagerId, command.MembersIds);
        }
        catch (DomainException ex)
        {
            LogHelper.BusinessRuleFailure(log, "Team creation ", $"{ex.Message}", null);
            throw new HandlerException(400, ex.Message, "Bad Request", "Domain Validation Error");
        }
        await teamRepository.CreateTeamAsync(team, cancellationToken);
        LogHelper.Info($"✅ Team {team.Name} has been created successfully.", log);
        foreach (var domainEvent in team.DomainEvents)
        {
            await _mediator.Publish(
                new DomainEventNotification<IDomainEvent>(domainEvent),
                cancellationToken
            );
        }
        team.ClearDomainEvents();
        return mapper.Map<TeamDto>(team);
    }

    public static double GetCommonMembersStats(
        IEnumerable<Guid> newTeamMembers,
        List<Team> existingTeams
    )
    {
        if (newTeamMembers == null || newTeamMembers.Count() == 0)
            throw new DomainException("The new team must have at least two member.");

        if (existingTeams == null || existingTeams.Count == 0)
            return 0;

        double maxPercent = 0;

        foreach (var existingTeam in existingTeams)
        {
            var common = existingTeam.MembersIds.Select(m => m.Value).Intersect(newTeamMembers).Count();
            var universe = existingTeam.MembersIds.Select(m => m.Value).Union(newTeamMembers).Count();
            double percent = (double)common / universe * 100;

            if (percent > maxPercent)
                maxPercent = percent;
        }
        return maxPercent;
    }
}
