using MediatR;
using Teams.API.Layer.DTOs;
namespace Teams.APP.Layer.CQRS.Commands;

public class CreateTeamCommand : IRequest<TeamDto>
{
    public string? Name { get; }
    public Guid TeamManagerId { get; }
    public List<Guid> MemberId { get; } = new();

    public CreateTeamCommand( string name, Guid teamManagerId, List<Guid> memberId)
    {
        Name = name;
        TeamManagerId = teamManagerId;
        MemberId = memberId;
    }
}
