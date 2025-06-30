using MediatR;
using Teams.API.Layer.DTOs;

namespace Teams.APP.Layer.CQRS.Commands;

public class UpdateTeamCommand : IRequest<TeamRequestDto>
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public Guid TeamManagerId { get; set; }
    public List<Guid> MemberId { get; set; } = new();

    public UpdateTeamCommand() { }

    public UpdateTeamCommand(Guid identifier, string name, Guid teamManagerId, List<Guid> memberId)
    {
        Id = identifier;
        Name = name;
        TeamManagerId = teamManagerId;
        MemberId = memberId;
    }
}
