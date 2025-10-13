using Humanizer;
using NodaTime;
using Teams.CORE.Layer.BusinessExceptions;
using Teams.CORE.Layer.CoreEvents;
using Teams.CORE.Layer.Entities.GeneralValueObjects;
using Teams.CORE.Layer.Entities.TeamAggregate.TeamValueObjects;


namespace Teams.CORE.Layer.Entities.TeamAggregate;

/// <summary>
/// Team state definitions:
/// Draft= 0    : Équipe en cours de création (moins de 3 membres ou pas de manager)
/// Active= 1   : Équipe valide (≥ 3 membres + 1 manager)
/// Archived= 2 : Équipe inactive ou restée non valide après un certain délai
/// </summary>
public enum TeamState
{
    Draft = 0,
    Active = 1,
    Archived = 2
}
public class Team : AggregateEntity, IAggregateRoot
{
    private TeamName _name;
    public TeamName Name => _name;
    private MemberId _teamManagerId;
    public MemberId TeamManagerId => _teamManagerId;
    private readonly HashSet<MemberId> _members = new();
    public IReadOnlyCollection<MemberId> MembersIds => _members;
    public TeamState State { get; private set; } = TeamState.Draft;
    public ProjectAssignmentState ProjectState { get; private set; } = ProjectAssignmentState.Unassigned;
    public ProjectAssociation? Project { get; private set; }
    public double AverageProductivity { get; private set; }
    public double TauxTurnover { get; private set; }
    private const int ValidityPeriodInDays = 250; // 250 pour les tests | Durée de validité standard en secondes (150 jours)
    private const int MaturityThresholdInDays = 280; // Seuil de maturité en secondes (180 jours)
    private int ExtraDays { get; set; } = 0;
    public LocalizationDateTime TeamCreationDate { get; private set; }
    public LocalizationDateTime TeamExpirationDate { get; private set; }
    public DateTime LastActivityDate { get; set; }
    public LocalizationDateTime Expiration => TeamCreationDate.Plus(Duration.FromSeconds(ValidityPeriodInDays + ExtraDays));
    /// <summary>
    /// Méthode interne pour obtenir la date actuelle à partir de l'horloge encapsulée.
    /// </summary>
    private LocalizationDateTime GetCurrentDateTime() => LocalizationDateTime.FromInstant(SystemClock.Instance.GetCurrentInstant()); // la date actuelle à 2heures de retard
    public bool IsTeamExpired() => GetCurrentDateTime().Value.ToInstant() >= Expiration.Value.ToInstant() && State != TeamState.Archived;


    #region Constructors
#pragma warning disable CS8618
    /// <summary>
    /// Constructeur pour Entity Framework Core
    /// Nécessaire pour la matérialisation par EF Core
    /// </summary>
    /// <remarks>
    /// Ce constructeur est requis par Entity Framework Core pour la matérialisation.
    /// </remarks>
    public Team()
    {
    }
#pragma warning restore CS8618

    /// <summary>
    /// Constructeur privé pour forcer l'utilisation de la méthode factory Create.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="teamManagerId"></param>
    /// <param name="members"></param>
    /// <param name="clock"></param>
    /// <exception cref="DomainException"></exception>
    /// <returns></returns>
    /// <remarks>
    /// Le constructeur initialise les propriétés de l'équipe, y compris la date d'expiration basée
    /// Sur la date de création et la période de validité standard.
    /// La validation des données de l'équipe est effectuée via la méthode ValidateTeamData.
    /// </remarks>
    private Team(
        Guid id,
        string name,
        Guid teamManagerId,
        IEnumerable<Guid> members,
        IClock clock
    )
    {
        Id = id;
        _name = TeamName.Create(name);
        _teamManagerId = new MemberId(teamManagerId);
        _members = members.Select(m => new MemberId(m)).ToHashSet();
        TeamCreationDate = LocalizationDateTime.Now(clock);
        TeamExpirationDate = TeamCreationDate.Plus(Duration.FromSeconds(ValidityPeriodInDays));

    }


    /// <summary>
    /// Calculates the maximum percentage of common members between a new team and a collection of existing teams.
    /// </summary>
    /// <param name="newTeamMembers">The list of members (as <see cref="Guid"/>) for the new team being created.</param>
    /// <param name="existingTeams">The collection of existing teams to compare against.</param>
    /// <returns>
    /// A <see cref="double"/> representing the highest percentage of overlap in members 
    /// between the new team and any existing team. Returns 0 if no existing teams are provided.
    /// </returns>
    /// <exception cref="DomainException">
    /// Thrown when <paramref name="newTeamMembers"/> is null or contains fewer than two members.
    /// </exception>
    private static double GetCommonMembersStats(IEnumerable<Guid> newTeamMembers, IEnumerable<Team> existingTeams)
    {
        if (newTeamMembers == null || newTeamMembers.Count() == 0)
            throw new DomainException("The new team must have at least two member.");

        if (existingTeams == null || existingTeams.Count() == 0)
            return 0;

        double maxPercent = 0;
        foreach (var team in existingTeams)
        {
            var common = team.MembersIds.Select(m => m.Value).Intersect(newTeamMembers).Count();
            var universe = team.MembersIds.Select(m => m.Value).Union(newTeamMembers).Count();
            double percent = (double)common / universe * 100;
            if (percent > maxPercent)
                maxPercent = percent;
        }
        return maxPercent;
    }

    /// <summary>
    /// Factory method to create a new <see cref="Team"/> while enforcing domain invariants.
    /// </summary>
    /// <param name="name">The unique name of the team to be created.</param>
    /// <param name="teamManagerId">The identifier of the team manager.</param>
    /// <param name="memberIds">The collection of member identifiers to include in the team.</param>
    /// <param name="teams">The collection of existing teams used for validation checks.</param>
    /// <returns>A newly created and valid <see cref="Team"/> instance.</returns>
    /// <exception cref="DomainException">
    /// Thrown when:
    /// <list type="bullet">
    /// <item><description> A team with the same name already exists. </description></item>
    /// <item><description> The manager already manages more than 3 teams. </description></item>
    /// <item><description> A team with the exact same members and manager already exists. </description></item>
    /// <item><description> The new team shares more than 50% of its members with an existing team. </description></item>
    /// </list>
    /// </exception>
    public static Team Create(
        string name,
        Guid teamManagerId,
        IEnumerable<Guid> memberIds,
        IEnumerable<Team> teams
    )
    {
        if (teams.Any(t => t.Name.Value.Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException($"A team with the name '{name}' already exists.");
        if (teams.Count(t => t.TeamManagerId.Value == teamManagerId) > 3)
            throw new DomainException("A manager cannot manage more than 3 teams.");
        if (
            teams.Any(t =>
                t.MembersIds.Count == memberIds.Count()
                && !t.MembersIds.Select(m => m.Value).Except(memberIds).Any()
                && t.TeamManagerId.Value == teamManagerId))
            throw new DomainException("A team with exactly the same members and manager already exists.");

        if (GetCommonMembersStats(memberIds, teams) >= 50)
            throw new DomainException("Cannot create a team with more than 50% common members with existing team.");

        var clock = SystemClock.Instance;
        var team = new Team(Guid.NewGuid(), name, teamManagerId, memberIds.ToHashSet(), clock);
        team.ValidateTeamInvariants();
        team.RecalculateStates();
        team.AddDomainEvent(new TeamCreatedEvent(team.Id));
        return team;
    }

    /// <summary>
    /// Update team details: name, manager, members.
    /// Cannot update an expired team.
    /// Validates changes and recalculates team state.
    /// </summary>
    /// <param name="newName"></param>
    /// <param name="newManagerId"></param>
    /// <param name="newMemberIds"></param>
    /// <exception cref="DomainException"></exception>
    public void UpdateTeam(string newName, Guid newManagerId, IEnumerable<Guid> newMemberIds)
    {
        if (IsTeamExpired())
            throw new DomainException("Cannot update an expired team.");

        bool isSameName = _name.Equals(TeamName.Create(newName));
        bool sameMembers = _members.SetEquals(newMemberIds.Select(m => new MemberId(m)));
        bool sameManager = _teamManagerId.Equals(new MemberId(newManagerId));

        if (isSameName && sameMembers && sameManager)
            throw new DomainException("No changes detected in the team details.");
        _name = TeamName.Create(newName);
        _teamManagerId = new MemberId(newManagerId);

        _members.Clear();
        _members.UnionWith(newMemberIds.Select(m => new MemberId(m)));
        ValidateTeamInvariants();
        RecalculateStates();
    }
    #endregion

    #region State Computation
    /// <summary>
    /// Compute the current state of the team based on its members and expiration.
    /// Draft if less than 3 members or no manager.
    /// Archived if past expiration date.
    /// Active otherwise.
    /// </summary>
    private TeamState ComputedTeamState
    {
        get
        {
            if (_members.Count < 3 || !_members.Contains(_teamManagerId))
                return TeamState.Draft;

            if (IsTeamExpired())
                return TeamState.Archived;

            return TeamState.Active;
        }
    }
    /// <summary>
    /// Recalculate and update the team's state and project state.
    /// This method should be called after any operation that modifies the team's members,
    /// manager, or project association to ensure the states are accurate.
    /// </summary>
    public void RecalculateStates()
    {
        State = ComputedTeamState;
        if (Project != null)
            ProjectState = Project.ComputedProjectState;
        else
            ProjectState = ProjectAssignmentState.Unassigned;
    }

    /// <summary>
    /// Verify if the team has any active or suspended project dependencies.
    /// if there are active projects, extend the team's expiration date accordingly.
    /// If there are no dependencies, return false.
    /// </summary>
    /// <returns></returns>
    public bool HasAnyDependencies()
    {
        if (Project == null || Project.IsEmpty())
            return false;

        bool hasDependencies = Project.HasActiveProject() || Project.HasSuspendedProject();
        if (Project.HasActiveProject())
        {
            TeamExpirationDate = LocalizationDateTime.FromInstant(Instant.FromDateTimeUtc(Project.GetprojectMaxEndDate().ToUniversalTime()));
            return true;
        }
        return hasDependencies;
    }

    /// <summary>
    /// Determine if the team is mature based on its creation date and maturity threshold.
    /// Only active teams can be evaluated for maturity.
    /// Maturity is defined as having existed for at least the maturity threshold duration.
    /// An exception is thrown if the team is not active.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="DomainException"></exception>
    public bool IsMature()
    {
        if (State != TeamState.Active)
            throw new DomainException("Only active teams can be evaluated for maturity.");

        if (!((GetCurrentDateTime().Value.ToInstant() - TeamCreationDate.Value.ToInstant()).TotalSeconds >= MaturityThresholdInDays)) // C'est 180 jours pour les tests on a mis 180 secondes
            return false;
        return true;
    }
    /// <summary>
    /// Archive the team if it has exceeded its validity period.
    /// Only teams that are past their expiration date can be archived.
    /// An exception is thrown if the team is not yet eligible for archiving.
    /// </summary>
    /// <exception cref="DomainException"></exception>
    /// <remarks>
    /// Archiving a team changes its state to Archived.
    /// This action is irreversible and should be performed with caution.
    /// </remarks>
    public void ArchiveTeam()
    {
        if (!IsTeamExpired())
            throw new DomainException("Team has not yet exceeded the validity period.");
        State = TeamState.Archived;
        AddDomainEvent(new TeamArchiveEvent(Id, Name.Value, TeamExpirationDate.ToInstant(), Guid.NewGuid()));
    }
    #endregion

    #region Business Logic
    /// <summary>
    /// Validate team invariants:
    /// - At least 3 members including the manager.
    /// - No more than 10 members.
    /// - Unique members.
    /// - Manager must be a member.
    /// Throws DomainException if any invariant is violated.
    /// </summary>
    /// <exception cref="DomainException"></exception>
    /// <remarks>
    /// This method is called during team creation and updates to ensure the team remains valid.
    /// </remarks>
    private void ValidateTeamInvariants()
    {
        if (_members.Count < 3)
            throw new DomainException("A team must have at least 3 members including team manager.");

        if (_members.Count > 10)
            throw new DomainException("A team cannot have more than 10 members.");

        if (_members.Distinct().Count() != _members.Count)
            throw new DomainException("Team members must be unique.");

        if (!_members.Contains(_teamManagerId))
            throw new DomainException("The manager must be one of the team members.");
    }
    /// <summary>
    /// Assign a project to the team.
    /// Validates project details and updates team expiration if necessary.
    /// Throws DomainException if any validation fails.
    /// </summary>
    /// <param name="project"></param>
    /// <exception cref="DomainException"></exception>
    /// <remarks>
    /// This method ensures that the project is valid and aligns with the team's creation date and manager.
    /// It also extends the team's expiration date based on the project's start date.
    /// </remarks>
    public void AssignProject(ProjectAssociation project)
    {
        if (project.IsEmpty())
            throw new DomainException("Project association data cannot be null");

        if (!project.HasActiveProject())
            throw new DomainException("Project must be active to be associated with a team.");

        if (project.TeamName != Name.Value)
            throw new DomainException($"Project associated with team {project.TeamName} does not match current team {Name}.");

        if (new MemberId(project.TeamManagerId) != _teamManagerId)
            throw new DomainException($"Project manager {project.TeamManagerId} does not match current team manager {_teamManagerId}.");

        if (project.GetprojectStartDate() < TeamCreationDate.ToDateTimeUtc())
            throw new DomainException($"Project start date {project.GetprojectStartDate()} cannot be earlier than team creation date {TeamCreationDate}");

        if (project.Details.Count > 3)
            throw new DomainException("A team cannot be associated with more than 3 projects.");

        Project = project;
        var delay = Project.GetprojectStartDate() - TeamCreationDate.ToDateTimeUtc();
        if (delay.TotalDays > 7)
            throw new DomainException(
                $"Project start date {project.GetprojectStartDate()} must be within 7 days of team creation date {TeamCreationDate}."
            );
        ExtraDays = 150;
        TeamExpirationDate = TeamExpirationDate.Plus(Duration.FromSeconds(ExtraDays));
        RecalculateStates();
        AddDomainEvent(new ProjectDateChangedEvent(Id));
    }

    /// <summary>
    /// Remove expired projects from the team.
    /// Updates team state and triggers domain event if necessary.
    /// </summary>
    /// <remarks>
    /// This method checks the project's details and removes any that have expired.
    /// It then recalculates the team's state and triggers a domain event to notify of the change.
    /// </remarks>
    public void RemoveExpiredProjects()
    {
        if (Project == null) return;

        Project.RemoveExpiredDetails();
        AddDomainEvent(new ProjectDateChangedEvent(Id));
        RecalculateStates();
    }

    /// <summary>
    /// Remove suspended projects from the team by project name.
    /// Updates team state and triggers domain event if necessary.
    /// </summary>
    /// <param name="projectName"></param>
    /// <remarks>
    /// This method checks the project's details and removes any that are marked as suspended.
    /// It then recalculates the team's state and triggers a domain event to notify of the change.
    /// </remarks>
    public void RemoveSuspendedProjects(string projectName)
    {
        if (Project == null) return;

        Project.TobeSuspended(projectName);
        AddDomainEvent(new ProjectDateChangedEvent(Id));
        RecalculateStates();
    }

    /// <summary>
    /// Add a new member to the team.
    /// Validates that the member is not already in the team, that the team does not exceed
    /// the maximum number of members, and that the member being added is not the manager.
    /// Throws DomainException if any validation fails.
    /// </summary>
    /// <param name="memberId"></param>
    /// <exception cref="DomainException"></exception>
    /// <remarks>
    /// This method ensures that the team remains valid after adding a new member.
    /// It also triggers a domain event to notify of the member addition.
    /// </remarks>
    public void AddMember(Guid memberId)
    {
        var vo = new MemberId(memberId);
        if (_members.Contains(vo))
            throw new DomainException("Member already exists in the team.");

        if (_members.Count > 10)
            throw new DomainException("A team cannot have more than 10 members.");

        _members.Add(vo);
        AddDomainEvent(new TeamMemberAddedEvent(Id, memberId));
        RecalculateStates();
    }
    /// <summary>
    /// Remove a member from the team.
    /// Validates that the member exists in the team, that the member being removed is not  the manager,
    /// and that the team does not fall below the minimum number of members.
    /// Throws DomainException if any validation fails.
    /// </summary>
    /// <param name="memberId"></param>
    /// <exception cref="DomainException"></exception>
    /// <remarks>
    /// This method ensures that the team remains valid after removing a member.
    /// It also triggers a domain event to notify of the member removal.
    /// </remarks>
    public void RemoveMemberSafely(Guid memberId)
    {
        var vo = new MemberId(memberId);
        if (vo == _teamManagerId)
            throw new DomainException("Cannot remove the team manager from the team.");

        if (!_members.Contains(vo))
            throw new DomainException("Member not found in the team.");

        if (_members.Count == 3)
            throw new DomainException("A team cannot have fewer than 3 members.");

        _members.Remove(vo);
        AddDomainEvent(new TeamMemberRemoveEvent(Id, memberId));
        RecalculateStates();
    }
    /// <summary>
    /// Change the team manager to a new member.
    /// Validates that the new manager is a member of the team and not an empty GUID
    /// Throws DomainException if any validation fails.
    /// </summary>
    /// <param name="newManagerId"></param>
    /// <exception cref="DomainException"></exception>
    /// <remarks>
    /// This method updates the team manager and triggers a domain event to notify of the change.
    /// It also recalculates the team's state to ensure it remains valid.
    /// </remarks>
    public void ChangeTeamManager(Guid newManagerId)
    {
        var managerId = new MemberId(newManagerId);
        if (newManagerId == Guid.Empty)
            throw new DomainException("New team manager ID cannot be empty.");

        if (!_members.Contains(managerId))
            throw new DomainException("New team manager must be a member of the team.");

        _teamManagerId = managerId;
        // AddDomainEvent(new TeamManagerChangedEvent(Id, newTeamManagerId));
        RecalculateStates();
    }
    #endregion
}
