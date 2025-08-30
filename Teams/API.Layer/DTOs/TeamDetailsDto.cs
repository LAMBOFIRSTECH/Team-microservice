using Teams.CORE.Layer.Entities;

namespace Teams.API.Layer.DTOs;

public class TeamDetailsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid ManagerId { get; set; }
    public List<Guid> MembersId { get; set; } = new List<Guid>();
    public List<Guid>? AssociatedProjectId { get; set; }
    public TeamState State { get; set; }
    public bool ActiveAssociatedProject { get; set; }
    public DateTime TeamCreationDate { get; set; }
    public DateTime TeamExpirationDate { get; set; }
    public DateTime? LastActivityDate { get; set; }
    public DateTime? ProjectStartDate { get; set; }
    public DateTime? ProjectEndDate { get; set; }
    public bool IsExpired { get; set; }
    public double? TauxTurnOver { get; set; }
    public double? AverageProductivity { get; set; }

    public TeamDetailsDto() { }

    public TeamDetailsDto(
        Guid managerId,
        string teamName,
        List<Guid> memberIds,
        DateTime teamCreationDate,
        DateTime teamExpirationDate,
        List<Guid>? associatedProjectId = null,
        TeamState state = TeamState.Active,
        bool activeAssociatedProject = false,
        DateTime? lastActivityDate = null,
        DateTime? projectStartDate = null,
        DateTime? projectEndDate = null,
        bool isExpired = false,
        double? tauxTurnOver = null,
        double? averageProductivity = null
    )
    {
        ManagerId = managerId;
        Name = teamName;
        MembersId = memberIds;
        AssociatedProjectId = associatedProjectId;
        State = state;
        ActiveAssociatedProject = activeAssociatedProject;
        TeamCreationDate = teamCreationDate;
        TeamExpirationDate = teamExpirationDate;
        LastActivityDate = lastActivityDate;
        ProjectStartDate = projectStartDate;
        ProjectEndDate = projectEndDate;
        IsExpired = isExpired;
        TauxTurnOver = tauxTurnOver;
        AverageProductivity = averageProductivity;
    }
}
