using CORE.Entities.TeamAG;
using Teams.CORE.Entities.GeneralValueObjects;
using Teams.CORE.CoreEvents;
using Teams.CORE.Entities.TeamAG.VO;
namespace Teams.CORE.Entities.TeamAG;

public class Team : AggregateEntity, IAggregateRoot
{
    private StringValue _name;
    private ManagerId _teamManagerId;
    public StringValue Name => _name;
    public ManagerId TeamManagerId => _teamManagerId;
    private TeamState State = TeamState.Draft;
    public Percentage AverageProductivity { get; private set; }
    public Percentage TauxTurnover { get; private set; }
    public byte[]? CompositionHash { get; set; }
    public DateTimeOffset TeamCreationDate { get; init; }
    public DateTimeOffset LastActivityDate { get; private set; }
    public DateTimeOffset TeamExpirationDate { get; private set; }
    public ExtraDays ExtraDays { get; set; }
    public bool IsDeleted { get; private set; }
    public ProjectAssignmentState ProjectState => ProjectAssociation?.ComputedProjectState ?? ProjectAssignmentState.Unassigned;
    public ProjectAssociation? ProjectAssociation { get; set; }
    public ICollection<TeamEvent> TeamEvents { get; set; } = new List<TeamEvent>();
    private readonly HashSet<TeamMember> _members = new();
    public IReadOnlyCollection<TeamMember> TeamMembers => _members;
    public ICollection<TeamMembersHistory> TeamMembersHistories { get; set; } = new List<TeamMembersHistory>();
    public ICollection<TeamProductivityHistory> TeamProductivityHistories { get; set; } = new List<TeamProductivityHistory>();

    /// <summary>
    /// Represents the lifecycle state of a team.
    /// </summary>
    private enum TeamState : short
    {
        /// <summary>The team is in draft mode (fewer than 3 members or the manager is not part of the members).</summary>
        Draft = 0,
        /// <summary>The team is active and valid (at least 3 members, with the manager included as a member).</summary>
        Active = 1,
        /// <summary>The team is archived (inactive or remained invalid after the allowed grace period).</summary>
        Archived = 2
    }
}


// private const int _validityPeriodInDays = 250; // 250 pour les tests | Durée de validité standard en secondes (15 jours)
// private const int _maturityThresholdInDays = 280; // Seuil de maturité en secondes (180 jours)