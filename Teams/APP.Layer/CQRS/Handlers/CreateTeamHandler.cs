using AutoMapper;
using MediatR;
using Teams.API.Layer.DTOs;
using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.CQRS.Commands;
using Teams.APP.Layer.Helpers;
using Teams.CORE.Layer.BusinessExceptions;
using Teams.CORE.Layer.CoreServices;
using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.INFRA.Layer.Dispatchers;

namespace Teams.APP.Layer.CQRS.Handlers;
public class CreateTeamHandler(
    ITeamRepository teamRepository,
    TeamLifeCycleCoreService teamLifeCycleCoreService,
    IMapper mapper,
    ILogger<CreateTeamHandler> log,
    IDomainEventDispatcher dispatcher
) : IRequestHandler<CreateTeamCommand, TeamDto>
{
    public async Task<TeamDto> Handle(CreateTeamCommand command, CancellationToken cancellationToken)
    {
        var existingTeams = await teamRepository.GetAllTeamsAsync(cancellationToken);
        try
        {
            var team = await teamLifeCycleCoreService.CreateTeamAsync(command.Name, command.TeamManagerId, command.MembersIds, existingTeams);
            await teamRepository.CreateTeamAsync(team, cancellationToken);
            LogHelper.Info($"✅ Team {team.Name} has been created successfully.", log);
            await dispatcher.DispatchAsync(team.DomainEvents, cancellationToken);
            team.ClearDomainEvents(); // On enleve ça ici le UofW est dans le DbContext “Domain Events dispatching in the Infrastructure Layer” pure DDD
            return mapper.Map<TeamDto>(team);
        }
        catch (DomainException ex)
        {
            LogHelper.BusinessRuleFailure(log, "Team creation failed", ex.Message, null);

            throw HandlerException.DomainError(
                title: "Team creation failed",
                statusCode: 422, // La requete a été comprise cependant le traitement n'aboutit pas à cause d'une exception levée par le domaine.
                message: ex.Message,
                reason: "Domain Validation Error"
            );
        }

    }
}
