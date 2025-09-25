using Microsoft.CodeAnalysis;
using Teams.CORE.Layer.BusinessExceptions;
using Teams.CORE.Layer.CoreEvents;
using Teams.CORE.Layer.ValueObjects;

namespace Teams.CORE.Layer.Entities;

/// <summary>
/// Active          : Équipe ayant au moins trois membres et un manager (manager étant aussi un membre).
/// Incomplete      : Équipe constituée mais sans projet affecté pendant 15 jours à partir de la date de création.
/// Complete        : Équipe Incomplete et associée à un projet.
/// Suspended       : Équipe Complete dont tous les projets sont suspendus.
/// UnderReview     : Équipe suspendue en cours d’évaluation pour réaffectation.
/// ToBeUnassigned  : Équipe non réaffectée à son projet initial après révision.
/// Archivee        : Équipe restée incomplète au bout de 15 jours.
/// </summary>
public enum TeamState
{
    Active = 0,
    Incomplete = 1,
    Complete = 2,
    Suspended = 3,
    UnderReview = 4,
    ToBeUnassigned = 5,
    Archivee = 6,
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
    public TeamState State { get; private set; }
    public DateTime TeamCreationDate { get; private set; }
    public DateTime LastActivityDate { get; set; }
    public double AverageProductivity { get; private set; }
    public double TauxTurnover { get; private set; }
    public ProjectAssociation Project { get; private set; }
    private const int ValidityPeriodInDays = 150; // C'est 15 jours

    #region Constructors
#pragma warning disable CS8618
    public Team() { }
#pragma warning restore CS8618

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
    }
    public static Team Create(
         string name,
         Guid teamManagerId,
         IEnumerable<Guid> memberIds
     )
    {
        var team = new Team(Guid.NewGuid(), name, teamManagerId, memberIds.ToHashSet(), GetLocalDateTime());
        team.ValidateTeamData();
        team.AddDomainEvent(new TeamCreatedEvent(team.Id));
        return team;
    }
    public void UpdateTeam(string newName, Guid newManagerId, IEnumerable<Guid> newMemberIds)
    {
        ValidateTeamData();
        IsTeamExpired();
        bool isSameName = _name.Equals(TeamName.Create(newName));
        bool sameMembers = _members.SequenceEqual(newMemberIds.Select(m => new MemberId(m)).ToHashSet());
        bool sameManager = TeamManagerId.Equals(newManagerId);
        if (isSameName && sameMembers && sameManager)
            throw new DomainException("No changes detected in the team details.");

        _name = TeamName.Create(newName);
        _teamManagerId = new MemberId(newManagerId);
        _members.Clear();
        _members.UnionWith(newMemberIds.Select(m => new MemberId(m)).ToHashSet());
        Project!.HasActiveProject(); // = false; Check s'il y' un projet actif
    }

    #endregion

    #region State Management
    public TeamState ComputedState
    {
        get
        {
            ValidateTeamData();
            // Pas de projet → équipe juste active mais pas complète
            if (Project == null || Project.GetprojectStartDate() == DateTime.MinValue || Project.GetprojectEndDate() == DateTime.MinValue)
                return TeamState.Incomplete;

            // Projet expiré → désaffectée
            if (Project.GetprojectEndDate() <= GetLocalDateTime())
                return TeamState.ToBeUnassigned; // À revoir
            // Pas de projet associé au bout de 15 jours
            if (IsTeamExpired())
                return TeamState.Archivee;
            // Trop vieux → archivé (pas encore implémenté)
            // if ((GetLocalDateTime() - LastActivityDate).TotalDays > ValidityPeriodInDays)
            //     return TeamState.Archivee;

            // Problèmes de perf → en révision (pas encore implémenté)
            // if (AverageProductivity < 0.4 || TauxTurnover > 0.5)
            //     return TeamState.UnderReview;

            // Tout va bien → équipe complète
            return TeamState.Complete;
        }
    }
    #endregion

    #region Businness Logic
    public bool HasAnyDependencies()
    {
        if (State == TeamState.Complete)
        {
            ExtraDays = 150;
            return true;
        }
        return false;
    }
    public bool IsMature()
    {
        if (State != TeamState.Incomplete && State != TeamState.Complete)
            throw new DomainException("Only active or complete teams can be mature.");

        if (!((GetLocalDateTime() - TeamCreationDate).TotalSeconds >= 180)) // C'est 180 jours pour les tests on a mis 180 secondes
            return false;

        AddDomainEvent(new TeamMaturityEvent(Id));
        return true;
    }

    public void Suspend()
    {
        if (State != TeamState.Active)
            throw new DomainException("Only active teams can be suspended.");

        State = TeamState.Suspended;
    }
    private void ValidateTeamData()
    {

        if (_members.Count < 3)
            throw new DomainException(
                "A team must have at least 3 members including team manager."
            );

        if (_members.Count > 10)
            throw new DomainException("A team cannot have more than 10 members.");

        if (_members.Distinct().Count() != _members.Count)
            throw new DomainException("Team members must be unique.");

        if (!_members.Contains(_teamManagerId))
            throw new DomainException("The manager must be one of the team members.");
        State = TeamState.Active;
    }

    public bool IsTeamExpired() =>
        GetLocalDateTime() >= ExpirationDate && State != TeamState.Archivee;

    public void ArchiveTeam()
    {
        if (!IsTeamExpired())
            throw new DomainException("Team has not yet exceeded the validity period.");

        State = TeamState.Archivee;
        AddDomainEvent(new TeamArchiveEvent(Id));
    }
    public void AddMember(Guid memberId)
    {
        var vo = new MemberId(memberId);
        if (_members.Contains(vo))
            throw new DomainException("Member already exists in the team.");

        if (_members.Count > 10)
            throw new DomainException("A team cannot have more than 10 members.");

        _members.Add(vo);
        AddDomainEvent(new TeamMemberAddedEvent(Id, memberId)); // Use dans le handler d'ajout de membres
    }
    public void RemoveMemberSafely(Guid memberId)
    {
        var vo = new MemberId(memberId);
        if (vo == _teamManagerId)
            throw new DomainException("Cannot remove the team manager from the team.");
        if (!_members.Contains(vo))
            throw new DomainException("Member not found in the team.");
        if (_members.Count == 3)
            throw new DomainException("A team cannot have least than 3 members.");

        _members.Remove(vo);
        AddDomainEvent(new TeamMemberRemoveEvent(Id, memberId));  // Use dans le handler de la suppression de membres
    }
    public void RemoveExpiredProjects()
    {
        if (Project == null) return;
        Project.RemoveExpiredDetails();

        AddDomainEvent(new ProjectDateChangedEvent(Id));
        if (Project.IsEmpty())
            RecalculateState();
    }

    public void RemoveSuspendedProjects(string projectName)
    {
        if (Project == null) return;
        Project.TobeSuspended(projectName);
        AddDomainEvent(new ProjectDateChangedEvent(Id));
        if (Project.IsEmpty())
            RecalculateState();

    }
    public void AssignProject(ProjectAssociation project)
    {
        if (project.IsEmpty())
            throw new DomainException("Project association data cannot be null");

        if (!project.HasActiveProject())
            throw new DomainException("Project must be active to be associated with a team.");

        if (project.TeamName != Name.Value)
            throw new DomainException(
                $"Project associated with team {project.TeamName} does not match current team {Name}."
            );

        if (new MemberId(project.TeamManagerId) != _teamManagerId)
            throw new DomainException(
                $"Project associated with team manager {project.TeamManagerId} does not match current team manager {_teamManagerId}."
            );

        if (project.GetprojectStartDate() < TeamCreationDate)
            throw new DomainException(
                $"Project start date {project.GetprojectStartDate()} cannot be earlier than team creation date {TeamCreationDate}"
            );

        if (State != TeamState.Incomplete && State != TeamState.Complete)
            throw new DomainException(
                "Only Incomplete or complete team can be associated to 1 or 3 projects."
            );

        if (project.Details.Count > 3)
            throw new DomainException("A team cannot be associated with more than 3 projects.");

        Project = project;
        var delay = Project.GetprojectStartDate() - TeamCreationDate;
        if (delay.TotalDays > 7)
            throw new DomainException(
                $"Project start date {project.GetprojectStartDate()} must be within 7 days of team creation date {TeamCreationDate}."
            );
        RecalculateState();
        AddDomainEvent(new ProjectDateChangedEvent(Id));
    }

    public void ChangeTeamManager(Guid newTeamManagerId)
    {
        var ManagerId = new MemberId(newTeamManagerId);
        if (newTeamManagerId == Guid.Empty)
            throw new DomainException("New team manager ID cannot be empty."); // A revoir

        if (!_members.Contains(ManagerId))
            throw new DomainException("New team manager must be a member of the team.");

        _teamManagerId = ManagerId; // Connaitre le type de contrat du nouveau manager eg. un stagiaire ne peut pas devenir manager
    }
    #endregion
    public static DateTime GetLocalDateTime() =>
            DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
    public void RecalculateState() => State = ComputedState;
    private int ExtraDays { get; set; } = 0;
    public DateTime ExpirationDate => TeamCreationDate.AddSeconds(ValidityPeriodInDays + ExtraDays);

    #region Domain Event Hnadling
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    private void AddDomainEvent(IDomainEvent @event) => _domainEvents.Add(@event);
    public void ClearDomainEvents() => _domainEvents.Clear();
    #endregion
}
