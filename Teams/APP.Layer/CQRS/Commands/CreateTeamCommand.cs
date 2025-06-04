using MediatR;
namespace Teams.APP.Layer.CQRS.Commands;
public class CreateTeamCommand : IRequest
{
    public Guid Id { get; }
    public string? Name { get; }
    public Guid TeamManagerId { get; }
    public List<Guid> MemberId { get; }

    public CreateTeamCommand(Guid id, string? name, Guid teamManagerId, List<Guid> memberId)
    {
        Id = id;
        Name = name;
        TeamManagerId = teamManagerId;
        MemberId = memberId;
    }
}
