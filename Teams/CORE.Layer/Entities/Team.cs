using System.ComponentModel.DataAnnotations;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using Teams.CORE.Layer.BusinessExceptions;
using Teams.CORE.Layer.ValueObjects;

namespace Teams.CORE.Layer.Entities;

public enum TeamState
{
    Incomplete = 0, // équipe créée et active mais pas de projet affecté
    Active = 1, // équipe ayant au moins 2 membres et un manager, avec ou sans projet associé
    Suspendue = 2, // équipe inactive, sans projet associé ou avec un projet associé mais inactif
    Archivee = 3, // équipe inactive, figée dans le temps, sans projet associé
    EnRevision = 4, // équipe en cours de révision, avec ou sans projet associé
    ADesaffecter = 5, // équipe active mais sans projet associé, en attente de désaffectation

    // pour les équipes qui ont un projet associé mais qui ne sont pas actives
    // (par exemple, un projet terminé ou suspendu)
    Complete = 6, // est une équipe active + projet associé
}

public class Team
{
    [Key]
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Guid TeamManagerId { get; private set; }

    // private readonly List<Guid> _memberIds = new();
    // public IReadOnlyCollection<Guid> MemberIds => _memberIds.AsReadOnly();
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
    private readonly DateTime _creationDate;
    public DateTime? CreationDate => _creationDate;
    public DateTime LastActivityDate { get; private set; }
    public double AverageProductivity { get; private set; }
    public double TauxTurnover { get; private set; }
    private DateTime? _projectStartDate;
    public DateTime? ProjectStartDate => _projectStartDate;
    private const int ValidityPeriodInDays = 90;

    public Team() { } //Pour EF

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
        _creationDate = creationDate;
    }

    public void AttachProjectToTeam(
        ProjectAssociation projectAssociation,
        bool activeAssociatedProject
    )
    {
        EnsureTeamIsWithinValidPeriod();
        if (activeAssociatedProject)
        {
            _projectStartDate = projectAssociation.ProjectStartDate;
            AssociateProject(projectAssociation);
            State = TeamState.Complete;
        }
    }

    private void AssociateProject(ProjectAssociation projectAssociation)
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
        if (State != TeamState.Active)
            throw new DomainException("Only active teams can have associated projects.");
        ActiveAssociatedProject = true;
        _projectStartDate = projectAssociation.ProjectStartDate;
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

    private void EnsureTeamIsWithinValidPeriod()
    {
        if (_creationDate.AddDays(ValidityPeriodInDays) <= DateTime.UtcNow)
        {
            State = TeamState.Archivee;
            throw new DomainException("Team has exceeded the 90-day validity period.");
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

    public static Team Create(
        string name,
        Guid teamManagerId,
        List<Guid> memberIds,
        List<Team> existingTeams,
        bool activeAssociatedProject,
        ProjectAssociation? projectAssociation = null,
        DateTime? creationDate = null
    )
    {
        var actualDate = creationDate ?? DateTime.UtcNow;
        if (existingTeams.Any(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException($"A team with the name '{name}' already exists.");
        var team = new Team(
            Guid.NewGuid(),
            name,
            teamManagerId,
            memberIds,
            actualDate,
            activeAssociatedProject ? TeamState.Complete : TeamState.Active,
            activeAssociatedProject
        );
        team.ValidateTeamData();
        team.EnsureTeamIsWithinValidPeriod();
        if (activeAssociatedProject)
        {
            team.AssociateProject(projectAssociation!);
            team.State = TeamState.Complete;
        }
        else
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

    // public static Team CreateWithProjectAssociated(
    //     string name,
    //     Guid teamManagerId,
    //     List<Guid> memberIds,
    //     List<Team> existingTeams,
    //     ProjectAssociation projectAssociation,
    //     bool activeAssociatedProject = true
    // )
    // {
    //     EnsureTeamIsWithinValidPeriod(_creationDate);
    //     var team = Create(name, teamManagerId, memberIds, existingTeams);
    //     if (team == null)
    //         throw new DomainException("Team creation failed due to invalid parameters.");
    //     if (team.Name != name)
    //         throw new DomainException("Team name does not match the provided name.");

    //     team.AssociatedProject(projectAssociation!);
    //     team.State = TeamState.Complete;
    //     return team;
    // }

    public void Suspend()
    {
        if (State != TeamState.Active)
            throw new DomainException("Only active teams can be suspended.");
        State = TeamState.Suspendue;
    }

    public void ResetState()
    {
        if (State == TeamState.Archivee)
            return; // State figé

        if (MembersIds.Count < 2 || TeamManagerId == null)
        {
            State = TeamState.Incomplete;
            return;
        }

        if (!ActiveAssociatedProject)
        {
            State = TeamState.Suspendue;
            return;
        }

        if (AverageProductivity < 0.4 || TauxTurnover > 0.5)
        {
            State = TeamState.EnRevision;
            return;
        }
        State = TeamState.Active;
    }

    public void TurnToArchiveIfInactive()
    {
        if (State == TeamState.Archivee)
            return;

        if ((DateTime.UtcNow - LastActivityDate).TotalDays > 90)
            State = TeamState.Archivee;
    }

    public void ChangeTeamManager(Guid newTeamManagerId)
    {
        if (newTeamManagerId == Guid.Empty)
            throw new DomainException("New team manager ID cannot be empty.");
        if (MembersIds.Contains(newTeamManagerId))
            throw new DomainException("New team manager must be a member of the team.");
        TeamManagerId = newTeamManagerId;
    }

    public void DeleteTeamMemberSafely(Guid memberId)
    {
        if (memberId == TeamManagerId)
            throw new DomainException("Cannot remove the team manager from the team.");

        if (!MembersIds.Contains(memberId))
            throw new DomainException("Member not found in the team.");
        MembersIds.Remove(memberId);
    }

    public void canMemberJoinNewTeam(TransfertMember transfertMember) { }
}
