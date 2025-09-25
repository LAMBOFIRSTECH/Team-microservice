using System.Collections.Generic;
using MediatR;
using Teams.API.Layer.DTOs;

namespace Teams.APP.Layer.CQRS.Commands;

public class CreateTeamCommand : IRequest<TeamDto>
{
    public string Name { get; set; }
    public Guid TeamManagerId { get; set; }
    public IEnumerable<Guid> MembersIds { get; set; }
    public CreateTeamCommand(string name, Guid teamManagerId, IEnumerable<Guid> membersId)
    {
        Name = name;
        TeamManagerId = teamManagerId;
        MembersIds = membersId;
    }
}
