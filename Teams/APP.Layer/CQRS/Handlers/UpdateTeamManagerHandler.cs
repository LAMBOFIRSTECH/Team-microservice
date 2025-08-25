using MediatR;
using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.CQRS.Commands;
using Teams.APP.Layer.Helpers;
using Teams.CORE.Layer.BusinessExceptions;
using Teams.CORE.Layer.Interfaces;

namespace Teams.APP.Layer.CQRS.Handlers;

public class UpdateTeamManagerHandler(
    ITeamRepository teamRepository,
    ILogger<UpdateTeamManagerHandler> logger
) : IRequestHandler<UpdateTeamManagerCommand, Unit>
{
    public async Task<Unit> Handle(
        UpdateTeamManagerCommand command,
        CancellationToken cancellationToken
    )
    {
        var team = await teamRepository.GetTeamByNameAndTeamManagerIdAsync(
            command.Name!,
            command.OldTeamManagerId,
            cancellationToken
        )!;
        var existingTeams = await teamRepository.GetAllTeamsAsync(cancellationToken);
        if (existingTeams == null)
        {
            LogHelper.Error(" ❌ No teams found in the repository.", logger);
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
                logger
            );
            throw new HandlerException(
                404,
                $"A team with Name : '{command.Name}' and Manager Id : '{command.OldTeamManagerId}' not found.",
                "Not Found",
                "Team ID not found"
            );
        }
        if (existingTeams.Count(t => t.TeamManagerId == command.NewTeamManagerId) > 3)
        {
            LogHelper.BusinessRuleFailure(
                logger,
                "Update Team Manager",
                "🚫 A manager cannot manage more than 3 teams.",
                null
            );
            throw new DomainException("A manager cannot manage more than 3 teams.");
        }
        if (team.TeamManagerId == command.NewTeamManagerId)
        {
            LogHelper.BusinessRuleFailure(
                logger,
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
                logger,
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
                logger
            );
        }
        catch (DomainException ex)
        {
            throw HandlerException.BadRequest(ex.Message, "Validation Error");
        }
        await teamRepository.UpdateTeamAsync(team, cancellationToken);
        return Unit.Value;
    }
}
