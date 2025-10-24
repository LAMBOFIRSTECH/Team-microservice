using MediatR;

namespace Teams.APP.Layer.CQRS.Commands;

public class DeleteTeamMemberCommand : IRequest
{
    public Guid MemberId { get; }
    public string TeamName { get; }

    public DeleteTeamMemberCommand(Guid memberId, string teamName)
    {
        MemberId = memberId;
        TeamName = teamName;
    }
}
