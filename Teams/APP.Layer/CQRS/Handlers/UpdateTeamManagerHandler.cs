using MediatR;
using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.CQRS.Commands;
using Teams.APP.Layer.Helpers;
using Teams.CORE.Layer.BusinessExceptions;
using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.INFRA.Layer.Interfaces;

namespace Teams.APP.Layer.CQRS.Handlers;

public class UpdateTeamManagerHandler(
    ITeamRepository _teamRepository,
    IUnitOfWork _unitOfWork,
    ILogger<UpdateTeamManagerHandler> _log
) : IRequestHandler<UpdateTeamManagerCommand, Unit>
{
    public async Task<Unit> Handle(
        UpdateTeamManagerCommand command,
        CancellationToken cancellationToken
    )
    {
        var team = await _teamRepository.GetTeamByNameAndTeamManagerIdAsync(
            command.Name!,
            command.OldTeamManagerId,
            cancellationToken
        )!;
        var existingTeams = _unitOfWork.TeamRepository.GetAll(cancellationToken);
        if (existingTeams == null)
        {
            LogHelper.Error(" ❌ No teams found in the repository.", _log);
            throw new HandlerException(
                404,
                "No teams found in the repository.",
                "Not Found",
                "Team Repository Empty"
            );
        }
        if (team == null)
        {
            LogHelper.Error(
                $"❌ Team with Name: {command.Name} and Old Team Manager Id: {command.OldTeamManagerId} not found.",
                _log
            );
            throw new HandlerException(
                404,
                $"A team with Name : '{command.Name}' and Manager Id : '{command.OldTeamManagerId}' not found.",
                "Not Found",
                "Team ID not found"
            );
        }
        if (existingTeams.Count(t => t.TeamManagerId.Value == command.NewTeamManagerId) > 3)
        {
            LogHelper.BusinessRuleFailure(
                _log,
                "Update Team Manager",
                "🚫 A manager cannot manage more than 3 teams.",
                null
            );
            throw new DomainException("A manager cannot manage more than 3 teams.");
        }
        if (team.TeamManagerId.Value == command.NewTeamManagerId)
        {
            LogHelper.BusinessRuleFailure(
                _log,
                "Update Team Manager",
                "🚫 The new manager is already the current manager of the team.",
                null
            );
            throw new DomainException(
                "The new manager is already the current manager of the team."
            );
        }
        if (
            command.ContratType.Equals("Stagiaire", StringComparison.OrdinalIgnoreCase)
            || command.ContratType.Equals("CDD", StringComparison.OrdinalIgnoreCase)
        )
        {
            LogHelper.BusinessRuleFailure(
                _log,
                "Update Team Manager",
                $"🚫 The member with contrat type {command.ContratType} cannot be assigned as a team manager.",
                null
            );
            throw new DomainException(
                $"🚫 The member with contrat type {command.ContratType} cannot be assigned as a team manager."
            );
        }
        try
        {
            team.ChangeTeamManager(command.NewTeamManagerId);
            LogHelper.Info(
                $"✅ Team manager changed successfully for team -- {command.Name} --",
                _log
            );
        }
        catch (DomainException ex)
        {
            throw HandlerException.BadRequest(ex.Message, "Validation Error");
        }
        _unitOfWork.TeamRepository.Update(team);
        return Unit.Value; // to be verified
    }
}
