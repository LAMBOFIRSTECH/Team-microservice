namespace Teams.CORE.Entities.TeamAG.VO;

public sealed class TeamMember 
{
    public TeamId? TeamId { get; set; }

    public MemberId? MemberId { get; set; }

    public Team Team { get; set; } = null!;
}
