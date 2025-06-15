using MediatR;

namespace Teams.APP.Layer.CQRS.Commands;

public class DeleteTeamCommand : IRequest
{
    public Guid Id { get; }
    public string Name { get; set; }

    public DeleteTeamCommand(Guid identifier, string name)
    {
        Name = name;
        Id = identifier;
    }
}
