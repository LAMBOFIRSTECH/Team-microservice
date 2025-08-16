using System.ComponentModel.DataAnnotations;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using Teams.CORE.Layer.BusinessExceptions;
using Teams.CORE.Layer.ValueObjects;

namespace Teams.CORE.Layer.Entities;

/// <summary>
/// Active       : Équipe ayant au moins deux membres et un manager.
/// Incomplete   : Équipe constituée mais sans projet affecté.
/// Complete     : Équipe active et associée à un projet.
/// Suspendue    : Équipe active dont le projet est suspendu.
/// EnRevision   : Équipe suspendue en cours d’évaluation pour réaffectation.
/// ADesaffecter : Équipe non réaffectée à son projet initial après révision.
/// Archivee     : Équipe restée incomplète pendant 15 jours.
/// </summary>
public enum TeamState
{
    Active = 0,
    Incomplete = 1,
    Complete = 2,
    Suspendue = 3,
    EnRevision = 4,
    ADesaffecter = 5,
    Archivee = 6,
}

public class Team
{
    // [Key]
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Guid TeamManagerId { get; private set; }
    public string? MemberIdSerialized { get; set; } = string.Empty;
    public List<Guid> MembersIds
    {
        get =>
            string.IsNullOrEmpty(MemberIdSerialized)
                ? new List<Guid>()
                : JsonConvert.DeserializeObject<List<Guid>>(MemberIdSerialized) ?? new List<Guid>();
        set => MemberIdSerialized = JsonConvert.SerializeObject(value);
    }

    public TeamState State { get; private set; } = TeamState.Incomplete;
    public bool ActiveAssociatedProject { get; private set; } = false;
    public DateTime TeamCreationDate { get; private set; }
    public DateTime LastActivityDate { get; set; }
    public double AverageProductivity { get; private set; }
    public double TauxTurnover { get; private set; }
    private DateTime? _projectStartDate;
    private DateTime? _projectEndDate;
    public DateTime? ProjectStartDate => _projectStartDate;
    public DateTime? ProjectEndDate => _projectEndDate;
    private const int ValidityPeriodInDays = 90;

    public static DateTime GetLocalDateTime() =>
        DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);

    public void RecalculateState() => State = ComputedState;

    public Team() { } // Pour EF

    private Team(
        Guid id,
        string name,
        Guid teamManagerId,
        List<Guid> memberIds,
        DateTime creationDate,
        TeamState state = TeamState.Incomplete,
        bool activeAssociatedProject = false
    )
    {
        Id = id;
        Name = name;
        TeamManagerId = teamManagerId;
        MembersIds = memberIds;
        State = state;
        ActiveAssociatedProject = activeAssociatedProject;
        TeamCreationDate = creationDate;
    }

    public bool IsProjectHasAnyDependencies(Team team)
    {
        if (team.State == TeamState.Complete)
            return true;
        return false;
    }

    public void RemoveProjectFromTeamWhenExpired(bool activeAssociatedProject)
    {
        if (State != TeamState.Complete && State != TeamState.Active)
            throw new DomainException(
                "Only active or complete teams can be disassociated from a project."
            );

        if (activeAssociatedProject)
            RecalculateState();
    }

    public void AttachProjectToTeam(
        ProjectAssociation projectAssociation,
        bool activeAssociatedProject
    )
    {
        if (projectAssociation == null)
            throw new DomainException("Project associated data cannot be null.");

        if (
            projectAssociation.TeamManagerId != TeamManagerId
            || projectAssociation.TeamName != Name
        )
            throw new DomainException(
                $"Project associated with team {projectAssociation.TeamName} does not match current team {Name}."
            );

        if (State != TeamState.Active && State != TeamState.Complete)
            throw new DomainException(
                "Only active or complete teams can been associated to 1 or 5 projects."
            );
        if (projectAssociation.ProjectStartDate < TeamCreationDate)
            throw new DomainException(
                $"Project start date {projectAssociation.ProjectStartDate} cannot be earlier than team creation date {TeamCreationDate}"
            );
        _projectEndDate = projectAssociation.ProjectEndDate;

        if (activeAssociatedProject)
        {
            _projectStartDate = projectAssociation.ProjectStartDate;
            var delay = _projectStartDate.Value - TeamCreationDate;
            if (delay.TotalDays > 7)
                throw new DomainException(
                    $"Project start date {_projectStartDate.Value} must be within 7 days of team creation date {TeamCreationDate}."
                );
            RecalculateState();
        }
    }

    private void ValidateTeamData()
    {
        if (MembersIds.Count < 2)
            throw new DomainException("A team must have at least 2 members.");

        if (MembersIds.Count > 10)
            throw new DomainException("A team cannot have more than 10 members.");

        if (MembersIds.Distinct().Count() != MembersIds.Count)
            throw new DomainException("Team members must be unique.");

        if (!MembersIds.Contains(TeamManagerId))
            throw new DomainException("The team manager must be one of the team members.");
    }

    public TeamState ComputedState
    {
        get
        {
            // Pas assez de membres → incomplet
            if (MembersIds.Count < 2)
                return TeamState.Incomplete;
            // Pas de projet → équipe juste active mais pas complète
            if (!_projectStartDate.HasValue || !_projectEndDate.HasValue)
                return TeamState.Active;
            // Projet expiré → désaffectée
            if (_projectEndDate <= GetLocalDateTime())
                return TeamState.ADesaffecter;
            // Trop vieux → archivé (pas encore implémenté)
            // if ((GetLocalDateTime() - LastActivityDate).TotalDays > ValidityPeriodInDays)
            //     return TeamState.Archivee;

            // // Problèmes de perf → en révision (pas encore implémenté)
            // if (AverageProductivity < 0.4 || TauxTurnover > 0.5)
            //     return TeamState.EnRevision;

            // Tout va bien → équipe complète
            return TeamState.Complete;
        }
    }

    private void EnsureTeamIsWithinValidPeriod()
    {
        if (TeamCreationDate.AddDays(ValidityPeriodInDays) <= GetLocalDateTime())
        {
            State = TeamState.Archivee;
            throw new DomainException($"Team has exceeded the 90-day validity period.Is too old.");
        }
    }

    public void AddMember(Guid memberId)
    {
        var members = MembersIds;
        if (members.Contains(memberId))
            throw new DomainException("Member already exists in the team.");

        members.Add(memberId);
        MembersIds = members;
    }

    public void DeleteTeamMemberSafely(Guid memberId)
    {
        if (memberId == TeamManagerId)
            throw new DomainException("Cannot remove the team manager from the team.");

        if (!MembersIds.Contains(memberId))
            throw new DomainException("Member not found in the team.");

        MembersIds.Remove(memberId);
    }

    public static Team Create(
        string name,
        Guid teamManagerId,
        List<Guid> memberIds,
        DateTime? creationDate = null
    )
    {
        var actualDate = creationDate ?? GetLocalDateTime();

        var team = new Team(Guid.NewGuid(), name, teamManagerId, memberIds, actualDate);
        team.ValidateTeamData();
        team.State = TeamState.Active;
        return team;
    }

    public void UpdateTeam(string newName, Guid newManagerId, List<Guid> newMemberIds)
    {
        ValidateTeamData();
        EnsureTeamIsWithinValidPeriod();
        bool isSameName = Name.Equals(newName, StringComparison.OrdinalIgnoreCase);
        bool sameMembers = MembersIds.SequenceEqual(newMemberIds);
        bool sameManager = TeamManagerId.Equals(newManagerId);
        if (isSameName && sameMembers && sameManager)
            throw new DomainException("No changes detected in the team details.");

        Name = newName;
        TeamManagerId = newManagerId;
        MembersIds.Clear();
        MembersIds.AddRange(newMemberIds);
        ActiveAssociatedProject = false;
    }

    public void Suspend()
    {
        if (State != TeamState.Active)
            throw new DomainException("Only active teams can be suspended.");

        State = TeamState.Suspendue;
    }

    public void ChangeTeamManager(Guid newTeamManagerId)
    {
        if (newTeamManagerId == Guid.Empty)
            throw new DomainException("New team manager ID cannot be empty.");

        if (!MembersIds.Contains(newTeamManagerId))
            throw new DomainException("New team manager must be a member of the team.");

        TeamManagerId = newTeamManagerId;
    }

    // Pour le projet associé sa logique métier doit etre défini ici et non pas dans le service sui orchestre car un projet associé vient changer l'etat de l'agrégat
}
