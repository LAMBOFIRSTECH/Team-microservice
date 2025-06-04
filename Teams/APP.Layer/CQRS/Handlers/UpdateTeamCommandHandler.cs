using MediatR;
using Teams.APP.Layer.CQRS.Commands;
using Teams.CORE.Layer.Interfaces;
namespace Teams.APP.Layer.CQRS.Handlers;

public class UpdateTeamCommandHandler : IRequestHandler<UpdateTeamCommand>
{
    private readonly ITeamRepository teamRepository;
    public UpdateTeamCommandHandler(ITeamRepository teamRepository)
    {
        this.teamRepository = teamRepository;
    }

    public Task Handle(UpdateTeamCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
