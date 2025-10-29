using Microsoft.CodeAnalysis;
using Teams.CORE.Layer.BusinessExceptions;
using Teams.CORE.Layer.Entities.TeamAggregate.TeamValueObjects;

namespace Teams.CORE.Layer.Entities.TeamAggregate.InternalEntities;

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
    public Guid DetailId { get; init; } = Guid.NewGuid();
    public string ProjectName { get; private set; }
    public DateTimeOffset ProjectStartDate { get; private set; }
    public DateTimeOffset ProjectEndDate { get; private set; }
    public VoState State { get; }

    /// <summary>
    /// Constructor for EF Core
    /// This constructor is required by Entity Framework Core for materialization.
    /// It should not be used directly in application code.
    /// </summary>
    private Detail()
    {
        ProjectName = string.Empty;
        ProjectStartDate = DateTimeOffset.MinValue;
        ProjectEndDate = DateTimeOffset.MinValue;
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
        DateTimeOffset projectStartDate,
        DateTimeOffset projectEndDate,
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
    public Guid ProjectId { get; private set; }
    public Guid TeamManagerId { get; private set; }
    public string TeamName { get; private set; }
    private readonly List<Detail> _details = new();
    public IReadOnlyList<Detail> Details => _details;


    // Constructeur domaine
    /// <summary>
    /// Domain constructor for ProjectAssociation
    /// </summary>
    /// <param name="projectId">The unique identifier for the project association</param>
    /// <param name="teamManagerId">The ID of the team manager</param>
    /// <param name="teamName">The name of the team</param>
    /// <param name="details">The list of project details</param>
    /// <remarks>
    /// This constructor enforces domain rules and validations.
    /// </remarks>
    public ProjectAssociation(Guid projectId, Guid teamManagerId, string teamName, List<Detail> details)
    {
        ProjectId = projectId;
        TeamManagerId = teamManagerId;
        TeamName = teamName;
        if (details != null)
            _details.AddRange(details);
    }

    // Constructeur EF Core
    /// <summary>
    /// Constructor for EF Core
    /// This constructor is required by Entity Framework Core for materialization.
    /// It should not be used directly in application code.
    /// </summary>
    private ProjectAssociation()
    {
        ProjectId = Guid.Empty;
        TeamName = string.Empty;
        TeamManagerId = Guid.Empty;
    }

    public bool IsUnderReview { get; set; } = false;
   

    /// <summary>
    /// Assign a project to the team with validation checks.
    /// This method ensures that the project being assigned meets
    /// all necessary domain rules before association.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="teamManagerId"></param>
    /// <param name="CreatedAt"></param>
    /// <exception cref="DomainException"></exception> Created a sub DomainException class for project association errors.
    public void ValidateProjectAssignmentToTeam(TeamName name, MemberId teamManagerId, DateTimeOffset CreatedAt)
    {
        if (TeamManagerId == Guid.Empty && string.IsNullOrWhiteSpace(TeamName) && (Details == null || Details.Count == 0))
            throw new DomainException("Project association data cannot be null");

        if (!Details.Any(d => d.State == VoState.Active))
            throw new DomainException("Project must be active to be associated with a team.");

        if (TeamName != name.Value)
            throw new DomainException($"Project associated with team {TeamName} does not match current team {name}.");

        if (new MemberId(TeamManagerId) != teamManagerId)
            throw new DomainException($"Project manager {TeamManagerId} does not match current team manager {teamManagerId}.");

        if (Details.First().ProjectStartDate < CreatedAt)
            throw new DomainException($"Project start date {Details.First().ProjectStartDate} cannot be earlier than team creation date {CreatedAt}");

        if (Details.Count > 3)
            throw new DomainException("A team cannot be associated to more than 3 projects.");

        var delay = Details.First().ProjectStartDate - CreatedAt;
        if (delay.TotalDays > 7)
            throw new DomainException($"Project start date {Details.First().ProjectStartDate} must be within 7 days of team creation date {CreatedAt}.");
    }

    /// <summary>
    /// Derivation rule of the project's assignment state based on its details.
    /// </summary>
    public ProjectAssignmentState ComputedProjectState
    {
        get
        {
            ProjectAssociation Project = this;
            if (Project.Details.Any(d => d.State == VoState.Suspended))
                return ProjectAssignmentState.Suspended;

            if (Project.IsUnderReview)
                return ProjectAssignmentState.UnderReview;

            if (Details.Any(d => d.ProjectEndDate <= DateTimeOffset.Now))
                return ProjectAssignmentState.UnassignedAfterReview;

            return ProjectAssignmentState.Assigned;
        }
    }
    public void AddDetail(Detail detail)
    {
        if (detail == null)
            throw new ArgumentNullException(nameof(detail), "Project association must contain at least one project detail");

        _details.Add(detail);
    }
    public void RemoveDetail(Detail detail)
    {
        if (detail == null)
            throw new ArgumentNullException(nameof(detail), "Detail cannot be null");

        _details.Remove(detail);
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
        if (Details.Any(d => d.State == VoState.Suspended))
        {
            var suspended = Details
                .Where(d => d.State == VoState.Suspended)
                .ToList();
            if (suspended.Count == 0) return;
            suspended.ForEach(d => _details.Remove(d));
        }
        else return;
    }
    /// <summary>
    /// Remove details of projects that have expired.
    /// This method checks for any project details where the end date has passed
    /// and removes them from the association.
    /// </summary>
    ///     <remarks>
    /// This method helps maintain the integrity of the project association
    /// by ensuring that only active or relevant project details are retained.
    /// It should be called periodically or after certain operations to clean up
    /// expired project details.
    /// </remarks>
    public void RemoveExpiredDetails()
    {
        var expired = ExpiredProjects();
        if (expired.Count == 0) return;
        expired.ForEach(d => _details.Remove(d));
    }
    public List<Detail> ExpiredProjects()
    {
        if (!Details.Any(d => d.State == VoState.Active)) return new List<Detail>();
        var expired = Details
              .Where(d => d.ProjectEndDate <= DateTimeOffset.Now)
              .ToList();
        return expired;
    }
}
