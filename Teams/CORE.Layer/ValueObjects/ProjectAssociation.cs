namespace Teams.CORE.Layer.ValueObjects;

public class ProjectAssociation
{
    public Guid TeamManagerId { get; }
    public string TeamName { get; }
    public DateTime ProjectStartDate { get; }

    public ProjectAssociation(Guid teamManagerId, string teamName, DateTime projectStartDate)
    {
        TeamManagerId = teamManagerId;
        TeamName = teamName;
        ProjectStartDate = projectStartDate;
    }
}
