using MediatR;

namespace Teams.APP.Layer.CQRS.Commands;

public class UpdateTeamManagerCommand : IRequest<Unit>
{
    public string Name { get; }
    public Guid OldTeamManagerId { get; }
    public Guid NewTeamManagerId { get; }
    public string ContratType { get; }

    public UpdateTeamManagerCommand(
        string name,
        Guid oldTeamManagerId,
        Guid newTeamManagerId,
        string contratType
    )
    {
        Name = name;
        OldTeamManagerId = oldTeamManagerId;
        NewTeamManagerId = newTeamManagerId;
        ContratType = contratType;
    }
}
