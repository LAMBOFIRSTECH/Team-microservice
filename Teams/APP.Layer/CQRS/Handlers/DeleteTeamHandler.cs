using MediatR;
using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.CQRS.Commands;
using Teams.APP.Layer.Helpers;
using Teams.CORE.Layer.BusinessExceptions;
using Teams.CORE.Layer.Interfaces;

namespace Teams.APP.Layer.CQRS.Handlers;

public class DeleteTeamHandler(ITeamRepository teamRepository, ILogger<DeleteTeamHandler> log)
    : IRequestHandler<DeleteTeamCommand>
{
    public async Task Handle(DeleteTeamCommand command, CancellationToken cancellationToken)
    {
        var team = await teamRepository.GetTeamByIdAsync(command.Id)!;
        if (team == null)
            throw new HandlerException(
                404,
                $"A team with the Id '{command.Id}' not found.",
                "Not Found",
                "Team ID not found"
            );
        var result = team.IsProjectHasAnyDependencies(team);
        if (result)
        {
            LogHelper.BusinessRuleFailure(
                log,
                "Delete Team",
                $"🚫 The team {team.Name} has been associated to 1 or more projects.",
                null
            );

            throw new DomainException(
                $"The team {team.Name} has been associated to 1 or more projects."
            );
        }
        await teamRepository.DeleteTeamAsync(command.Id);
        LogHelper.Info($"✅ Team with Name {team.Name} has been deleted successfully.", log);
    }
}
