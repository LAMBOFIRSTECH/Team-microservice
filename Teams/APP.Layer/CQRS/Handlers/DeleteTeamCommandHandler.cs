using MediatR;
using Teams.CORE.Layer.Interfaces;
using Teams.APP.Layer.CQRS.Commands;
namespace Teams.APP.Layer.CQRS.Handlers;

public class DeleteTeamCommandHandler : IRequestHandler<DeleteTeamCommand>
{
    private readonly ITeamRepository teamRepository;
    public DeleteTeamCommandHandler(ITeamRepository teamRepository)
    {
        this.teamRepository = teamRepository;
    }

    public Task Handle(DeleteTeamCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
