using CORE.Entities.TeamAG;
using Teams.CORE.CoreEvents;

namespace Teams.CORE.Entities.TeamAG;
public sealed class ProjectAssociation
{
    public Guid ProjectId { get; private set; }
    public Guid TeamManagerId { get; private set; }
    public string TeamName { get; private set; }
    public ProjectAssignmentState State { get; private set; }
    public ProjectAssignmentState ComputedProjectState => this.State;
    public bool IsUnderReview { get; private set; }
    private readonly List<ProjectDetails> _details = new();
    public IReadOnlyList<ProjectDetails> Details => _details.AsReadOnly();

    /// <summary>
    /// Private constructor for ORM and serialization purposes. 
    /// Initializes the ProjectAssociation with default values, ensuring that the entity is in a valid state even when instantiated without parameters.
    /// This constructor is not intended for direct use in business logic.
    /// </summary>
    private ProjectAssociation()
    {
        ProjectId = Guid.Empty;
        TeamManagerId = Guid.Empty;
        TeamName = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the ProjectAssociation class with the specified project ID, team manager ID, team name, initial project details, and initial assignment state. Validates the input parameters to ensure they meet business rules.
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="teamManagerId"></param>
    /// <param name="teamName"></param>
    /// <param name="initialDetails"></param>
    /// <param name="initialState"></param>
    /// <exception cref="BusinessRuleException"></exception>
    internal ProjectAssociation(Guid projectId, Guid teamManagerId, string teamName, ICollection<ProjectDetails> initialDetails, ProjectAssignmentState initialState)
    {
        if (projectId == Guid.Empty)
            throw new BusinessRuleException(Errors.ErrorNature.Validation, "ProjectId cannot be empty.", "project_id_invalid");
        if (teamManagerId == Guid.Empty)
            throw new BusinessRuleException(Errors.ErrorNature.Validation, "TeamManagerId cannot be empty.", "team_manager_id_invalid");
        if (string.IsNullOrWhiteSpace(teamName))
            throw new BusinessRuleException(Errors.ErrorNature.Validation, "TeamName cannot be empty.", "team_name_invalid");
        if (initialDetails == null || initialDetails.Count == 0)
            throw new BusinessRuleException(Errors.ErrorNature.Validation, "At least one detail is required.", "project_details_required");
        if (initialDetails.Count > 3)
            throw new BusinessRuleException(Errors.ErrorNature.Validation, "A team cannot have more than 3 projects.", "team_too_many_projects");

        ProjectId = projectId;
        TeamManagerId = teamManagerId;
        TeamName = teamName;
        State = initialState;
        IsUnderReview = (initialState == ProjectAssignmentState.UnderReview);

        _details.AddRange(initialDetails);
    }

    /// <summary>
    /// Point d'entrée unique pour synchroniser l'état complet de l'entité depuis le DTO RabbitMQ (Idempotence)
    /// </summary>
    internal void SynchronizeFromExternalService(ProjectAssignmentState newState, ICollection<ProjectDetails> incomingDetails)
    {
        if (incomingDetails == null || incomingDetails.Count == 0)
            throw new BusinessRuleException(Errors.ErrorNature.Validation, "Synchronization failed: incoming details cannot be empty.", "sync_details_required");

        if (incomingDetails.Count > 3)
            throw new BusinessRuleException(Errors.ErrorNature.Validation,"A team cannot have more than 3 projects.", "team_too_many_projects");

        this.State = newState;
        this.IsUnderReview = (newState == ProjectAssignmentState.UnderReview);

        // Écrasement propre du snapshot précédent par les nouveaux Value Objects reçus
        _details.Clear();
        _details.AddRange(incomingDetails);
    }

    /// <summary>
    /// Validates the project assignment to a team based on the provided team name, current manager ID, and team creation date.
    /// </summary>
    /// <param name="teamNameValue"></param>
    /// <param name="currentManagerId"></param>
    /// <param name="teamCreatedAt"></param>
    /// <exception cref="BusinessRuleException"></exception>
    internal void ValidateProjectAssignmentToTeam(string teamNameValue, Guid currentManagerId, DateTimeOffset teamCreatedAt)
    {
        if (_details.Count == 0)
            throw new BusinessRuleException(Errors.ErrorNature.Validation, "No project details found.", "project_details_required");

        if (!_details.Any(d => d.State == ProjectDetails.ProjectDetailState.Active))
            throw new BusinessRuleException(Errors.ErrorNature.Validation, "At least one active project is required.", "project_active_required");

        if (!TeamName.Equals(teamNameValue, StringComparison.Ordinal))
            throw new BusinessRuleException(Errors.ErrorNature.Validation, "Team name mismatch.", "team_name_mismatch");

        if (TeamManagerId != currentManagerId)
            throw new BusinessRuleException(Errors.ErrorNature.Validation, "Team manager mismatch.", "team_manager_mismatch");

        var firstDetail = _details.First();

        if (firstDetail.StartDate < teamCreatedAt)
            throw new BusinessRuleException(Errors.ErrorNature.Validation, "Project cannot start before team creation.", "project_start_date_invalid");

        if (firstDetail.StartDate > teamCreatedAt.AddDays(7))
            throw new BusinessRuleException(Errors.ErrorNature.Validation, "Project must start within 7 days of team creation.", "project_start_date_invalid");
    }

    /// <summary>
    /// Mask as suspended a specific project detail locally, ensuring that only the designated team manager can perform this action.
    /// </summary>
    /// <param name="projectName"></param>
    /// <param name="managerId"></param>
    /// <param name="suspendedAt"></param>
    /// <exception cref="BusinessRuleException"></exception>
    internal ProjectSuspendedFromTeamDomainEvent MarkAsSuspendedLocally(string projectName, Guid managerId, DateTimeOffset suspendedAt)
    {
        if (TeamManagerId != managerId)
            throw new BusinessRuleException(Errors.ErrorNature.Validation, "Only the designated team manager can modify project details.", "unauthorized_manager");

        var targetIndex = _details.FindIndex(d => d.State == ProjectDetails.ProjectDetailState.Active && d.ProjectName.Equals(projectName, StringComparison.Ordinal));

        if (targetIndex == -1)
            throw new BusinessRuleException(Errors.ErrorNature.Validation, $"No active project detail found with the name '{projectName}' to suspend.", "active_project_detail_not_found");

        var oldDetail = _details[targetIndex];

        // Remplacement à l'index sans duplication de ligne
        _details[targetIndex] = new ProjectDetails(oldDetail.ProjectName, oldDetail.StartDate, oldDetail.EndDate, ProjectDetails.ProjectDetailState.Suspended, suspendedAt);

        this.State = ProjectAssignmentState.Suspended;
        return new ProjectSuspendedFromTeamDomainEvent(
            ProjectId: this.ProjectId,
            ProjectName: projectName,
            TeamManagerId: managerId,
            OccurredOn: suspendedAt
        );
    }

    /// <summary>
    /// Creates and returns a ProjectRemovedFromTeamDomainEvent without storing it, ensuring that only the designated team manager can perform this action.
    /// Validates the existence of a suspended project detail before removal.
    /// </summary>
    /// <param name="projectName"></param>
    /// <param name="managerId"></param>
    /// <param name="removedAt"></param>
    /// <returns></returns>
    /// <exception cref="BusinessRuleException"></exception>
    internal ProjectRemovedFromTeamDomainEvent ProjectRemovedFromTeamDomain(string projectName, Guid managerId, DateTimeOffset removedAt)
    {
        if (TeamManagerId != managerId)
            throw new BusinessRuleException(Errors.ErrorNature.Validation, "Only the designated team manager can modify project details.", "unauthorized_manager");

        var target = _details.FirstOrDefault(d => d.State == ProjectDetails.ProjectDetailState.Suspended && d.ProjectName.Equals(projectName, StringComparison.Ordinal));

        if (target is null)
            throw new BusinessRuleException(Errors.ErrorNature.Validation, $"No suspended project detail found with the name '{projectName}' to remove.", "suspended_project_detail_not_found");

        _details.Remove(target);
        if (_details.Count == 0) this.State = ProjectAssignmentState.Unassigned;
        

        // On instancie et on retourne l'événement sans le stocker ici
        return new ProjectRemovedFromTeamDomainEvent(
            ProjectId: this.ProjectId,
            ProjectName: projectName,
            TeamManagerId: managerId,
            OccurredOn: removedAt
        );
    }

    /// <summary>
    /// Returns the expired projects based on the reference date.
    /// </summary>
    /// <param name="referenceDateTime"></param>
    /// <returns></returns>
    internal IReadOnlyList<ProjectDetails> GetExpiredProjects(DateTimeOffset referenceDateTime)
    {
        return _details
            .Where(d => d.EndDate <= referenceDateTime)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Adds a new project detail to the association, ensuring that the total number of details does not exceed 3.
    /// </summary>
    /// <param name="detail"></param>
    /// <exception cref="BusinessRuleException"></exception>
    internal void AddDetail(ProjectDetails detail)
    {
        if (detail is null)
            throw new BusinessRuleException(Errors.ErrorNature.Validation, "Detail cannot be null.", "project_detail_invalid");

        if (_details.Count >= 3)
            throw new BusinessRuleException(Errors.ErrorNature.Validation, "A team cannot have more than 3 projects.", "team_too_many_projects");

        _details.Add(detail);
    }

    /// <summary>
    /// Removes expired project details based on the provided reference date. If all details are removed, the overall state is updated to Unassigned.>
    /// </summary>
    /// <param name="referenceDateTime"></param>
    internal void RemoveExpiredDetails(DateTimeOffset referenceDateTime)
    {
        var expired = _details
            .Where(d => d.EndDate <= referenceDateTime)
            .ToList();

        foreach (var d in expired)
        {
            _details.Remove(d);
        }

        if (_details.Count == 0 && this.State != ProjectAssignmentState.Unassigned)
            this.State = ProjectAssignmentState.Unassigned;
        
    }

}
