using AutoMapper;
using MediatR;
using Teams.API.Layer.DTOs;
using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.CQRS.Commands;
using Teams.APP.Layer.Helpers;
using Teams.CORE.Layer.BusinessExceptions;
using Teams.CORE.Layer.CoreServices;
using Teams.INFRA.Layer.Dispatchers;
using Teams.INFRA.Layer.Interfaces;

namespace Teams.APP.Layer.CQRS.Handlers;

public class CreateTeamHandler(
    IUnitOfWork _unitOfWork,
    TeamLifeCycleCoreService _teamLifeCycleCoreService,
    IMapper _mapper,
    ILogger<CreateTeamHandler> _log,
    IDomainEventDispatcher _dispatcher
) : IRequestHandler<CreateTeamCommand, TeamDto>
{
    public async Task<TeamDto> Handle(CreateTeamCommand command, CancellationToken cancellationToken)
    {
        var existingTeams =  _unitOfWork.TeamRepository.GetAll();
        try
        {
            var team = await _teamLifeCycleCoreService.CreateTeamAsync(command.Name, command.TeamManagerId, command.MembersIds, existingTeams);
            await _unitOfWork.TeamRepository.Create(team, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken); // commit après transaction
            LogHelper.Info($"✅ Team {team.Name} has been created successfully.", _log);
            await _dispatcher.DispatchAsync(team.DomainEvents, cancellationToken);
            team.ClearDomainEvents(); // On enleve ça ici le UofW est dans le DbContext “Domain Events dispatching in the Infrastructure Layer” pure DDD
            return _mapper.Map<TeamDto>(team);
        }
        catch (DomainException ex)
        {
            LogHelper.BusinessRuleFailure(_log, "Team creation failed", ex.Message, null);
            throw HandlerException.DomainError(
                title: "Team creation failed",
                statusCode: 422, // La requete a été comprise cependant le traitement n'aboutit pas à cause d'une exception levée par le domaine.
                message: ex.Message,
                reason: "Domain Validation Error"
            );
        }

    }
}