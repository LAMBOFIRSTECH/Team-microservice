using MediatR;

namespace Teams.APP.Layer.CQRS.Commands;

public class DeleteTeamMemberCommand : IRequest
{
    public Guid MemberId { get; set; }
    public string TeamName { get; set; }

    public DeleteTeamMemberCommand(Guid memberId, string teamName)
    {
        MemberId = memberId;
        TeamName = teamName;
    }
}
