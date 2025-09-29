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
    public async Task<TeamDto> Handle(CreateTeamCommand command, CancellationToken cancellationToken)
    {
        var existingTeams = await teamRepository.GetAllTeamsAsync(cancellationToken);
        try
        {
            var team = Team.Create(command.Name, command.TeamManagerId, command.MembersIds, existingTeams);
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
        catch (DomainException ex)
        {
            LogHelper.BusinessRuleFailure(log, "Team creation ", $"{ex.Message}", null);
            throw HandlerException.BadRequest(ex.Message, "Domain Validation Error");
        }
    }
}
