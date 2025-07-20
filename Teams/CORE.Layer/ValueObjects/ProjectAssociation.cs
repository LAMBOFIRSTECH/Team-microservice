namespace Teams.CORE.Layer.ValueObjects;

public enum ProjectState
{
    Active,
    Suspended,
    Terminated,
}

public class ProjectAssociation
{
    public Guid TeamManagerId { get; }
    public string TeamName { get; }
    public DateTime ProjectStartDate { get; }
    public ProjectState State { get; }

    public ProjectAssociation(
        Guid teamManagerId,
        string teamName,
        DateTime projectStartDate,
        ProjectState state
    )
    {
        TeamManagerId = teamManagerId;
        TeamName = teamName;
        ProjectStartDate = projectStartDate;
        State = state;
    }
}
