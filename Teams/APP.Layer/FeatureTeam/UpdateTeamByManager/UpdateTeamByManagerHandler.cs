using MediatR;
using Teams.APP.Layer.Exceptions;
using Teams.APP.Layer.Helpers;
using Teams.CORE.Layer.Exceptions;
using Teams.CORE.Layer.Entities.TeamAggregate;
using Microsoft.Extensions.Logging;
using Teams.CORE.Layer.CoreInterfaces;

namespace Teams.APP.Layer.FeatureTeam.UpdateTeamByManager;

public class UpdateTeamByManagerHandler(
    ITeamRepository _teamRepository,
    IUnitOfWork _unitOfWork,
    ILogger<UpdateTeamByManagerHandler> _log
) : IRequestHandler<UpdateTeamByManagerCommand, Unit>
{
    public async Task<Unit> Handle(
        UpdateTeamByManagerCommand command,
        CancellationToken cancellationToken
    )
    {
        var team = await _teamRepository.GetTeamByNameAndTeamManagerIdAsync(
            command.TeamName!,
            command.OldTeamManagerId,
            cancellationToken
        )!;
        var existingTeams = await _unitOfWork.TeamRepository.GetAllAsync(cancellationToken);
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
                $"❌ Team with Name: {command.TeamName} and Old Team Manager Id: {command.OldTeamManagerId} not found.",
                _log
            );
            throw new HandlerException(
                404,
                $"A team with Name : '{command.TeamName}' and Manager Id : '{command.OldTeamManagerId}' not found.",
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
            throw new BusinessRuleException("A manager cannot manage more than 3 teams.");
        }
        if (team.TeamManagerId.Value == command.NewTeamManagerId)
        {
            LogHelper.BusinessRuleFailure(
                _log,
                "Update Team Manager",
                "🚫 The new manager is already the current manager of the team.",
                null
            );
            throw new BusinessRuleException(
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
            throw new BusinessRuleException(
                $"🚫 The member with contrat type {command.ContratType} cannot be assigned as a team manager."
            );
        }
        try
        {
            team.ChangeTeamManager(command.NewTeamManagerId);
            LogHelper.Info(
                $"✅ Team manager changed successfully for team -- {command.TeamName} --",
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
