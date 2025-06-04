using MediatR;
using Teams.APP.Layer.CQRS.Commands;
using Teams.CORE.Layer.Interfaces;
namespace Teams.APP.Layer.CQRS.Handlers;

public class CreateTeamCommandHandler : IRequestHandler<CreateTeamCommand>
{
    private readonly ITeamRepository teamRepository;
    public CreateTeamCommandHandler(ITeamRepository teamRepository)
    {
        this.teamRepository = teamRepository;
    }
    public async Task Handle(CreateTeamCommand request, CancellationToken cancellationToken)
    {
    }
}
