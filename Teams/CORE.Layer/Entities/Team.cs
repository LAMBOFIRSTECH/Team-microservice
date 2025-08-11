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
    private readonly DateTime _creationDate;
    public DateTime? CreationDate => _creationDate;
    public DateTime LastActivityDate { get; private set; }
    public double AverageProductivity { get; private set; }
    public double TauxTurnover { get; private set; }
    private DateTime? _projectStartDate;
    public DateTime? ProjectStartDate => _projectStartDate;
    private const int ValidityPeriodInDays = 90;

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

    public void DeleteTeamMemberSafely(Guid memberId)
    {
        if (memberId == TeamManagerId)
            throw new DomainException("Cannot remove the team manager from the team.");

        if (!MembersIds.Contains(memberId))
            throw new DomainException("Member not found in the team.");

        MembersIds.Remove(memberId);
    }

    public static double GetCommonMembersStats(List<Guid> newTeamMembers, List<Team> existingTeams)
    {
        if (newTeamMembers == null || newTeamMembers.Count == 0)
            throw new DomainException("The new team must have at least one member.");

        if (existingTeams == null || existingTeams.Count == 0)
            return 0; // Pas d'équipes existantes → pas de comparaison

        double maxPercent = 0;

        foreach (var existingTeam in existingTeams)
        {
            var common = existingTeam.MembersIds.Intersect(newTeamMembers).Count();
            var universe = existingTeam.MembersIds.Union(newTeamMembers).Count();
            double percent = (double)common / universe * 100;

            if (percent > maxPercent)
                maxPercent = percent;
        }

        return maxPercent;
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

        if (existingTeams.Count(t => t.TeamManagerId == teamManagerId) > 3)
            throw new DomainException("A manager cannot manage more than 3 teams.");

        if (
            existingTeams.Any(t =>
                t.MembersIds.Count == memberIds.Count
                && !t.MembersIds.Except(memberIds).Any()
                && t.TeamManagerId == teamManagerId
            )
        )
            throw new DomainException(
                "A team with exactly the same members and manager already exists."
            );
        var maxCommonPercent = GetCommonMembersStats(memberIds, existingTeams);
        if (maxCommonPercent >= 50)
            throw new DomainException(
                "Cannot create a team with more than 50% common members with existing teams."
            );

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

        if (MembersIds.Count < 2)
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

        if (!MembersIds.Contains(newTeamManagerId))
            throw new DomainException("New team manager must be a member of the team.");

        TeamManagerId = newTeamManagerId;
    }

    // Pour le projet associé sa logique métier doit etre défini ici et non pas dans le service sui orchestre car un projet associé vient changer l'etat de l'agrégat
}
