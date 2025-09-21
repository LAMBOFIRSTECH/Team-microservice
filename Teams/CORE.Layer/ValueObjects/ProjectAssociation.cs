namespace Teams.CORE.Layer.ValueObjects;

public enum ProjectState
{
    Active = 0,
    Suspended = 1,
}

public class Detail
{
    public string ProjectName { get; }
    public DateTime ProjectStartDate { get; }
    public DateTime ProjectEndDate { get; }
    public ProjectState State { get; }

    public Detail(
        string projectName,
        DateTime projectStartDate,
        DateTime projectEndDate,
        ProjectState state
    )
    {
        ProjectName = projectName;
        ProjectStartDate = projectStartDate;
        ProjectEndDate = projectEndDate;
        State = state;
    }
}

public class ProjectAssociation
{
    public Guid TeamManagerId { get; }
    public string TeamName { get; } // changer ça plustard use le Vo
    public List<Detail> Details { get; }

    public ProjectAssociation(Guid teamManagerId, string teamName, List<Detail> details)
    {
        TeamManagerId = teamManagerId;
        TeamName = teamName;
        Details = details;
    }
}
