using CORE.Entities.TeamAG.VO;
namespace Teams.CORE.Entities.TeamAG.VO;

public sealed class TeamProductivityHistory 
{
    public TeamProductivityHistoryId Id { get;}

    public TeamId TeamId { get;  }

    public double Productivity { get; }

    public DateTime MeasuredAt { get; private set; }

    public  Team Team { get; private set; } = null!;

     public TeamProductivityHistory(TeamProductivityHistoryId id, TeamId teamId, double productivity, DateTime measuredAt)
    {
        Id = id;
        TeamId = teamId;
        Productivity = productivity;
        MeasuredAt = measuredAt;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private TeamProductivityHistory() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
}
