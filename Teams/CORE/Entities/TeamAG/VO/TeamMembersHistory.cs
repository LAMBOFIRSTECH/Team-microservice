using CORE.Entities.TeamAG.VO;
namespace Teams.CORE.Entities.TeamAG.VO;
public sealed class TeamMembersHistory
{
    public TeamMembersHistoryId Id { get; }
    public TeamId TeamId { get; }
    public MemberId MemberId { get; }
    public DateTime JoinDate { get; }
    public DateTime? LeaveDate { get; private set; }
    public Team Team { get; private set; } = null!;

    public TeamMembersHistory(TeamMembersHistoryId id, TeamId teamId, MemberId memberId, DateTime joinDate)
    {
        Id = id;
        TeamId = teamId;
        MemberId = memberId;
        JoinDate = joinDate;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private TeamMembersHistory() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
}