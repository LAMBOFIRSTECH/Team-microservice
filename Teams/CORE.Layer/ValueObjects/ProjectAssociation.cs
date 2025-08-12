namespace Teams.CORE.Layer.ValueObjects;

public enum ProjectState
{
    Active = 0,
    Suspended = 1,
    Terminated = 2,
}

public class ProjectAssociation
{
    public Guid TeamManagerId { get; }
    public string TeamName { get; }
    public string ProjectName { get; }
    public DateTime ProjectStartDate { get; }
    public DateTime ProjectEndDate { get; }
    public ProjectState State { get; }

    public ProjectAssociation(
        Guid teamManagerId,
        string teamName,
        string projectName,
        DateTime projectStartDate,
        DateTime projectEndDate,
        ProjectState state
    )
    {
        TeamManagerId = teamManagerId;
        TeamName = teamName;
        ProjectName = projectName;
        ProjectStartDate = projectStartDate;
        ProjectEndDate = projectEndDate;
        State = state;
    }
}
