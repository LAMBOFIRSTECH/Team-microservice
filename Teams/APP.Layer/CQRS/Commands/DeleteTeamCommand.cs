using MediatR;
namespace Teams.APP.Layer.CQRS.Commands;

public class DeleteTeamCommand : IRequest
{
    public Guid Id { get; }
    public DeleteTeamCommand(Guid identifier)
    {
        Id = identifier;
    }
}
