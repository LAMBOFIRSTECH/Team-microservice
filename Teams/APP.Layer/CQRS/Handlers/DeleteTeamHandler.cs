using MediatR;
using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.CQRS.Commands;
using Teams.CORE.Layer.Interfaces;

namespace Teams.APP.Layer.CQRS.Handlers;

public class DeleteTeamHandler(ITeamRepository teamRepository) : IRequestHandler<DeleteTeamCommand>
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
        await teamRepository.DeleteTeamAsync(command.Id);
    }
}
