namespace Teams.CORE.Layer.ValueObjects;

/// <summary>
/// Statut d'un projet venant d'un service externe
/// Active = 0       : Projet actif dans le microservice de projets
/// Suspended = 1    : Projet suspendu dans le microsevice de projets
/// </summary>
public enum VoState
{
    Active = 0,
    Suspended = 1,
}

public class Detail
{
    public Guid Id { get; private set; } = Guid.NewGuid(); 
    public string ProjectName { get; private set; }
    public DateTime ProjectStartDate { get; private set; }
    public DateTime ProjectEndDate { get; private set; }
    public VoState State { get; }

    /// <summary>
    /// Constructor for EF Core
    /// This constructor is required by Entity Framework Core for materialization.
    /// It should not be used directly in application code.
    /// </summary>
    private Detail()
    {
        ProjectName = string.Empty;
        ProjectStartDate = DateTime.MinValue;
        ProjectEndDate = DateTime.MinValue;
        State = VoState.Active;
    }

    /// <summary>
    /// Domain constructor for Detail value object 
    /// This constructor enforces domain rules and validations.
    /// </summary>
    /// <param name="projectName"></param>
    /// <param name="projectStartDate"></param>
    /// <param name="projectEndDate"></param>
    /// <param name="state"></param>
    public Detail(
        string projectName,
        DateTime projectStartDate,
        DateTime projectEndDate,
        VoState state
    )
    {
        ProjectName = projectName;
        ProjectStartDate = projectStartDate;
        ProjectEndDate = projectEndDate;
        State = state;
    }
}
/// <summary>
/// Statut d'affectation projet d'une équipe
/// Unaffected = 0,           : Aucune affectation projet
/// Assigned = 1,             : Projet en cours
/// Suspended = 2,            : Projet(s) associé(s) suspendu(s)
/// UnderReview = 3,          : Projet en cours d’évaluation pour réaffectation
/// UnassignedAfterReview = 4 : Équipe restée sans projet après révision
/// </summary>
public enum ProjectAssignmentState
{
    Unassigned = 0,
    Assigned = 1,
    Suspended = 2,
    UnderReview = 3,
    UnassignedAfterReview = 4
}
public class ProjectAssociation
{
    public Guid TeamManagerId { get; private set; }
    public string TeamName { get; private set; }
    private readonly List<Detail> _details = new();
    public IReadOnlyList<Detail> Details => _details;
    

    // Constructeur domaine
    /// <summary>
    /// Domain constructor for ProjectAssociation
    /// </summary>
    /// <param name="teamManagerId">The ID of the team manager</param>
    /// <param name="teamName">The name of the team</param>
    /// <param name="details">The list of project details</param>
    /// <remarks>
    /// This constructor enforces domain rules and validations.
    /// </remarks>
    public ProjectAssociation(Guid teamManagerId, string teamName, List<Detail> details)
    {
        TeamManagerId = teamManagerId;
        TeamName = teamName;
        _details = details;
    }

    // Constructeur EF Core
    /// <summary>
    /// Constructor for EF Core
    /// This constructor is required by Entity Framework Core for materialization.
    /// It should not be used directly in application code.
    /// </summary>
    private ProjectAssociation()
    {
        TeamName = string.Empty;
        TeamManagerId = Guid.Empty;
        _details = new List<Detail>();
    }

    public bool IsEmpty() => TeamManagerId == Guid.Empty && string.IsNullOrWhiteSpace(TeamName) && (Details == null || Details.Count == 0);
    public bool IsExpired() => Details.Any(d => d.ProjectEndDate <= DateTime.Now);
    public bool HasSuspendedProject() => Details.Any(d => d.State == VoState.Suspended);
    public bool HasActiveProject() => Details.Any(d => d.State == VoState.Active);
    public DateTime GetprojectStartDate() => Details.Select(p => p.ProjectStartDate).FirstOrDefault();
    public DateTime GetprojectEndDate() => Details.Select(p => p.ProjectEndDate).FirstOrDefault();
    public DateTime GetprojectMaxEndDate() => Details.Select(p => p.ProjectEndDate).Max();
    public ProjectAssignmentState ComputedProjectState
    {
        get
        {
            ProjectAssociation Project = this;
            if (Project == null || Project.IsEmpty())
                return ProjectAssignmentState.Unassigned;

            if (Project.HasSuspendedProject())
                return ProjectAssignmentState.Suspended;

            // if (Project.IsUnderReview)
            //     return ProjectAssignmentState.UnderReview; // plustard

            if (Project.IsExpired())
                return ProjectAssignmentState.UnassignedAfterReview;

            return ProjectAssignmentState.Assigned;
        }
    }
    public void AddDetail(Detail detail)
    {
        if (detail == null)
            throw new ArgumentNullException(nameof(detail), "Detail cannot be null");

        _details.Add(detail);
    }
    public void TobeSuspended(string projectName)
    {
        foreach (var detail in _details)
        {
            if (detail.State == VoState.Active && detail.ProjectName == projectName)
            {
                var suspendedDetail = new Detail(
                    detail.ProjectName,
                    detail.ProjectStartDate,
                    detail.ProjectEndDate,
                    VoState.Suspended
                );
                _details.Remove(detail);
                _details.Add(suspendedDetail);
                break;
            }
        }
        if (HasSuspendedProject())
        {
            var suspended = Details
                .Where(d => d.State == VoState.Suspended)
                .ToList();
            if (suspended.Count == 0) return;
            suspended.ForEach(d => _details.Remove(d));
        }
        else return;
    }
    public void RemoveExpiredDetails()
    {
        if (HasActiveProject())
        {
            var expired = Details
                .Where(d => d.ProjectEndDate <= DateTime.Now)
                .ToList();
            if (expired.Count == 0) return;
            expired.ForEach(d => _details.Remove(d));
        }
        else return;
    }
}
