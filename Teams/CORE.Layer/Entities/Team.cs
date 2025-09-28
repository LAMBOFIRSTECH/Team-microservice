using Teams.CORE.Layer.BusinessExceptions;
using Teams.CORE.Layer.CoreEvents;
using Teams.CORE.Layer.ValueObjects;

namespace Teams.CORE.Layer.Entities;

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

public class Team
{
    public Guid Id { get; private set; }
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
    private const int ValidityPeriodInDays = 250; // Durée de validité standard en secondes (150 jours)
    private const int MaturityThresholdInDays = 280; // Seuil de maturité en secondes (180 jours)
    private int ExtraDays { get; set; } = 0;
    public DateTime TeamCreationDate { get; private set; }
    public DateTime TeamExpirationDate { get; private set; }
    public DateTime LastActivityDate { get; set; }
    public DateTime ExpirationDate => TeamCreationDate.AddSeconds(ValidityPeriodInDays + ExtraDays);
    // Datetime.Now Couplage fort avec le systeme use IClock à la place
    #region Constructors
#pragma warning disable CS8618
    public Team() { }
#pragma warning restore CS8618

    /// <summary>
    /// Constructeur privé pour forcer l'utilisation de la méthode factory Create.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="teamManagerId"></param>
    /// <param name="members"></param>
    /// <param name="creationDate"></param>
    /// <exception cref="DomainException"></exception>
    /// <returns></returns>
    /// <remarks>
    /// Le constructeur initialise les propriétés de l'équipe, y compris la date d'expiration basée
    /// sur la date de création et la période de validité standard.
    /// La validation des données de l'équipe est effectuée via la méthode ValidateTeamData.
    /// </remarks>
    private Team(
        Guid id,
        string name,
        Guid teamManagerId,
        IEnumerable<Guid> members,
        DateTime creationDate
    )
    {
        Id = id;
        _name = TeamName.Create(name);
        _teamManagerId = new MemberId(teamManagerId);
        _members = members.Select(m => new MemberId(m)).ToHashSet();
        TeamCreationDate = creationDate;
        TeamExpirationDate = creationDate.AddSeconds(ValidityPeriodInDays);
    }

    public bool IsTeamExpired() =>
         GetLocalDateTime() >= ExpirationDate && State != TeamState.Archived;

    public static Team Create(
         string name,
         Guid teamManagerId,
         IEnumerable<Guid> memberIds
     )
    {
        var team = new Team(Guid.NewGuid(), name, teamManagerId, memberIds.ToHashSet(), GetLocalDateTime());
        team.ValidateTeamInvariants();
        team.RecalculateStates();
        team.AddDomainEvent(new TeamCreatedEvent(team.Id));
        return team;
    }
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
    public TeamState ComputedTeamState
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
            TeamExpirationDate = Project!.GetprojectMaxEndDate();
            return true;
        }
        return hasDependencies;
    }

    public bool IsMature()
    {
        if (State != TeamState.Active)
            throw new DomainException("Only active teams can be evaluated for maturity.");

        if (!((GetLocalDateTime() - TeamCreationDate).TotalSeconds >= MaturityThresholdInDays))
            return false;
        return true;
    }
    public void ArchiveTeam()
    {
        if (!IsTeamExpired())
            throw new DomainException("Team has not yet exceeded the validity period.");

        State = TeamState.Archived;
    }
    #endregion

    #region Business Logic
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

        if (project.GetprojectStartDate() < TeamCreationDate)
            throw new DomainException($"Project start date {project.GetprojectStartDate()} cannot be earlier than team creation date {TeamCreationDate}");

        if (project.Details.Count > 3)
            throw new DomainException("A team cannot be associated with more than 3 projects.");

        Project = project;
        var delay = Project.GetprojectStartDate() - TeamCreationDate;
        if (delay.TotalDays > 7)
            throw new DomainException(
                $"Project start date {project.GetprojectStartDate()} must be within 7 days of team creation date {TeamCreationDate}."
            );
        ExtraDays = 150;
        TeamExpirationDate = TeamExpirationDate.AddSeconds(ExtraDays);
        RecalculateStates();
        AddDomainEvent(new ProjectDateChangedEvent(Id));
    }

    public void RemoveExpiredProjects()
    {
        if (Project == null) return;

        Project.RemoveExpiredDetails();
        AddDomainEvent(new ProjectDateChangedEvent(Id));
        RecalculateStates();
    }

    public void RemoveSuspendedProjects(string projectName)
    {
        if (Project == null) return;

        Project.TobeSuspended(projectName);
        AddDomainEvent(new ProjectDateChangedEvent(Id));
        RecalculateStates();
    }

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

    public void ChangeTeamManager(Guid newTeamManagerId)
    {
        var managerId = new MemberId(newTeamManagerId);
        if (newTeamManagerId == Guid.Empty)
            throw new DomainException("New team manager ID cannot be empty.");

        if (!_members.Contains(managerId))
            throw new DomainException("New team manager must be a member of the team.");

        _teamManagerId = managerId;
        AddDomainEvent(new TeamManagerChangedEvent(Id, newTeamManagerId));
        RecalculateStates();
    }
    #endregion

    #region Domain Event Handling
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    private void AddDomainEvent(IDomainEvent @event) => _domainEvents.Add(@event);
    public void ClearDomainEvents() => _domainEvents.Clear();
    #endregion

    public static DateTime GetLocalDateTime() =>
        DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
}
