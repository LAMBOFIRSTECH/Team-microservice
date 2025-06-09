using MediatR;
using Teams.CORE.Layer.Interfaces;
using Teams.APP.Layer.CQRS.Commands;
using Teams.API.Layer.DTOs;
namespace Teams.APP.Layer.CQRS.Handlers;

public class DeleteTeamCommandHandler : IRequestHandler<DeleteTeamCommand>
{
    private readonly ITeamRepository teamRepository;
    public DeleteTeamCommandHandler(ITeamRepository teamRepository)
    {
        this.teamRepository = teamRepository;
    }

    public async Task Handle(DeleteTeamCommand request, CancellationToken cancellationToken)
    {
        var team = await teamRepository.GetTeamByIdAsync(request.Id)!;
        if (team == null)
            throw new KeyNotFoundException($"Team with ID {request.Id} not found.");

        await teamRepository.DeleteTeamAsync(request.Id);
    }
}
