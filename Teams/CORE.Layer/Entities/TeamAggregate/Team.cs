using NodaTime;
using Teams.CORE.Layer.BusinessExceptions;
using Teams.CORE.Layer.CoreEvents;
using Teams.CORE.Layer.Entities.GeneralValueObjects;
using Teams.CORE.Layer.Entities.TeamAggregate.TeamValueObjects;
using Teams.CORE.Layer.Entities.TeamAggregate.InternalEntities;
using Teams.CORE.Layer.CommonExtensions;
using NodatimePackage.Classes;
using NodatimePackage.Models.Regions;

namespace Teams.CORE.Layer.Entities.TeamAggregate;

/// <summary>
/// Team state definitions:
/// Draft= 0    : Équipe en cours de création (moins de 3 membres ou pas de manager dans les membres)
/// Active= 1   : Équipe valide (≥ 3 membres + 1 manager étant aussi membre)
/// Archived= 2 : Équipe inactive ou restée non valide après un certain délai
/// </summary>
public enum TeamState
{
    Draft = 0,
    Active = 1,
    Archived = 2,
}

public class Team : AggregateEntity, IAggregateRoot
{
    private const int _validityPeriodInDays = 250; // 250 pour les tests | Durée de validité standard en secondes (15 jours)
    private const int _maturityThresholdInDays = 280; // Seuil de maturité en secondes (180 jours)
    private int _extraDays { get; set; } = 0;
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
    public DateTimeOffset TeamCreationDate { get; init; }
    public DateTimeOffset LastActivityDate { get; private set; }
    public DateTimeOffset Expiration => TeamCreationDate.AddSeconds(_validityPeriodInDays + _extraDays);
    public DateTimeOffset TeamExpirationDate { get; private set; }

    /// <summary>
    /// Get the current date and time as a LocalizationDateTime.
    /// </summary>
    private DateTimeOffset GetCurrentDateTime() => TimeOperations.GetCurrentTime("UTC").UtcDateTime;


    ///<summary>
    /// /// Determine if the team has exceeded its validity period.
    /// </summary>
    /// <returns></returns> 
    public bool IsTeamExpired()
      => TimeOperations.GetCurrentTime("UTC").UtcDateTime >= TeamExpirationDate.UtcDateTime && State != TeamState.Archived;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="timeZoneId"></param>
    /// <returns></returns>
    public DateTimeOffset GetExpirationDateInTimeZone(string timeZoneId)
            => timeZoneId.ConvertDatetimeIntoDateTimeOffset(TeamExpirationDate);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="timeZoneId"></param>
    /// <returns></returns>
    public DateTimeOffset GetLastActivityInTimeZone(string timeZoneId)
            => timeZoneId.ConvertDatetimeIntoDateTimeOffset(LastActivityDate);


    #region Constructors
#pragma warning disable CS8618
    /// <summary>
    /// Constructeur pour Entity Framework Core
    /// Nécessaire pour la matérialisation par EF Core
    /// </summary>
    /// <remarks>
    /// Ce constructeur est requis par Entity Framework Core pour la matérialisation.
    /// </remarks>
    public Team() { }
#pragma warning restore CS8618

    /// <summary>
    /// Constructeur privé pour forcer l'utilisation de la méthode factory Create.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="teamManagerId"></param>
    /// <param name="members"></param>
    /// <param name="createAt"></param>
    /// <exception cref="DomainException"></exception>
    /// <returns></returns>
    /// <remarks>
    /// Le constructeur initialise les propriétés de l'équipe, y compris la date d'expiration basée
    /// Sur la date de création et la période de validité standard.
    /// La validation des données de l'équipe est effectuée via la méthode ValidateTeamData.
    /// </remarks>
    private Team(Guid id, string name, Guid teamManagerId, IEnumerable<Guid> members, DateTimeOffset createAt)
    {
        Id = id;
        _name = TeamName.Create(name);
        _teamManagerId = new MemberId(teamManagerId);
        _members = members.Select(m => new MemberId(m)).ToHashSet();
        TeamCreationDate = createAt;
        TeamExpirationDate = Expiration;
        LastActivityDate = createAt;
    }
    #endregion
    #region Factory Method

    /// <summary>
    /// Factory method to create a new <see cref="Team"/> while enforcing domain invariants.
    /// </summary>
    /// <param name="name">The unique name of the team to be created.</param>
    /// <param name="teamManagerId">The identifier of the team manager.</param>
    /// <param name="memberIds">The collection of member identifiers to include in the team.</param>
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
    public static Team Create(string name, Guid teamManagerId, IEnumerable<Guid> memberIds)
    {
        var team = new Team(Guid.NewGuid(), name, teamManagerId, memberIds.ToHashSet(),  DateTimeOffset.Now);
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
    /// Derivation rule of the team's state based on its members and expiration.
    /// Compute the current state of the team based on its members and expiration.
    /// Draft if less than 3 members or no manager.
    /// Archived if past expiration date.
    /// Active otherwise.
    /// </summary>
    private TeamState ComputedTeamState
    {
        get
        {
            if (_members.Count < 3 || _members.Count >= 10 || !_members.Contains(_teamManagerId)) return TeamState.Draft;

            if (IsTeamExpired()) return TeamState.Archived;

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
    #endregion

    #region Team project deletion Methods
    /// <summary>
    /// Mark the team as deleted after ensuring it can be deleted.
    /// A team cannot be deleted if it has active or suspended project associations.
    /// </summary>
    public void MarkAsDeleted()
    {
        EnsureCanBeDeleted();
        AddDomainEvent(new TeamDeletedEvent(Id, Name.Value, TeamExpirationDate.UtcDateTime, Guid.NewGuid()));
    }

    public void EnsureCanBeDeleted()
    {
        if (Project != null && Project.DependencyExist())
            throw new DomainException($"The team '{Name}' cannot be deleted because it has active or suspended project associations.");
    }
    #endregion

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

        if (!((GetCurrentDateTime() - TeamCreationDate.UtcDateTime).TotalSeconds >= _maturityThresholdInDays))
            // C'est 180 jours pour les tests on a mis 180 secondes
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
        AddDomainEvent(
            new TeamArchiveEvent(Id, Name.Value, TeamExpirationDate, Guid.NewGuid())
        );
    }


    #region Business Logic
    #region Team Methods
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

    #region Project Association Methods
    /// <summary>
    /// Apply a grace period to the team's expiration date based on project attachment.
    /// This method extends the team's expiration date by the specified number of days.
    /// </summary>
    /// <param name="days"></param>
    public void ApplyProjectAttachmentGracePeriod(int days)
    {
        _extraDays += days;
        TeamExpirationDate = TeamExpirationDate.AddSeconds(days);
    }
    public void AssignProject(ProjectAssociation proj)
    {
        Project = proj;
        Project.ValidateProjectAssignmentToTeam(Name, _teamManagerId, TeamCreationDate);
        ApplyProjectAttachmentGracePeriod(_extraDays);
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
        Project?.RemoveExpiredDetails();
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
        if (Project == null)
            return;
        Project.TobeSuspended(projectName);
        AddDomainEvent(new ProjectDateChangedEvent(Id));
        RecalculateStates();
    }
    #endregion
    #endregion
}
