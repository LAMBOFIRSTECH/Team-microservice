using System.Collections.Generic;
using MediatR;
using Teams.API.Layer.DTOs;

namespace Teams.APP.Layer.CQRS.Commands;

public class CreateTeamCommand : IRequest<TeamDto>
{
    public string? Name { get; set; }
    public Guid TeamManagerId { get; set; }
    public List<Guid> MembersId { get; } = new();

    public CreateTeamCommand() { }

    public CreateTeamCommand(string name, Guid teamManagerId, List<Guid> membersId)
    {
        Name = name;
        TeamManagerId = teamManagerId;
        MembersId = membersId;
    }
}
