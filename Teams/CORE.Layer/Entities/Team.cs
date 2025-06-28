using System.ComponentModel.DataAnnotations;
using Microsoft.CodeAnalysis;
using Teams.APP.Layer.ExternalServicesDtos;
using Teams.CORE.Layer.BusinessExceptions;
using Teams.CORE.Layer.Models;
using Teams.CORE.Layer.ValueObjects;

namespace Teams.CORE.Layer.Entities;

public enum TeamState
{
    Incomplete = 0, // équipe créée et active mais pas de projet affecté
    Active = 1,
    Suspendue = 2,
    Archivee = 3,
    EnRevision = 4,
    ADesaffecter = 5,
    Complete = 6, // est une équipe active + projet associé
}

public class Team
{
    [Key]
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Guid TeamManagerId { get; private set; }
    private readonly List<Guid> _memberIds = new();
    public IReadOnlyCollection<Guid> MemberIds => _memberIds.AsReadOnly();
    public TeamState State { get; private set; } = TeamState.Incomplete;
    public DateTime LastActivityDate { get; private set; }
    public bool ActiveAssociatedProject { get; private set; }
    public double AverageProductivity { get; private set; }
    public double TauxTurnover { get; private set; }
    private DateTime? _projectStartDate;
    public DateTime? ProjectStartDate => _projectStartDate;

    private Team(
        Guid id,
        string name,
        Guid teamManagerId,
        List<Guid> memberIds,
        TeamState state = TeamState.Incomplete,
        bool activeAssociatedProject = false
    )
    {
        Id = id;
        Name = name;
        TeamManagerId = teamManagerId;
        _memberIds.AddRange(memberIds);
        State = state;
        ActiveAssociatedProject = activeAssociatedProject;
    }

    private static void ValidateTeamCreation(
        string name,
        Guid teamManagerId,
        List<Guid> memberIds,
        List<Team> existingTeams
    )
    {
        if (memberIds.Count < 2)
            throw new DomainException("A team must have at least 2 members.");
        if (memberIds.Count > 10)
            throw new DomainException("A team cannot have more than 10 members.");
        if (memberIds.Distinct().Count() != memberIds.Count)
            throw new DomainException("Team members must be unique.");
        if (!memberIds.Contains(teamManagerId))
            throw new DomainException("The team manager must be one of the team members.");
        if (existingTeams.Any(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException($"A team with the name '{name}' already exists.");
    }

    public void Activate()
    {
        if (_memberIds.Count < 2)
            throw new DomainException("A team must have at least 2 members to be activated.");
        if (!MemberIds.Contains(TeamManagerId))
            throw new DomainException(
                "The team manager must be a member of the team to activate it."
            );
        State = TeamState.Active;
    }

    public void IsAnyProjectAssociated(ProjectAssociation projectAssociation)
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
        _projectStartDate = projectAssociation.ProjectStartDate; // save the project start date in the team domain
    }

    public static Team Create(
        string name,
        Guid teamManagerId,
        List<Guid> memberIds,
        List<Team> existingTeams,
        bool activeAssociatedProject = false,
        ProjectAssociation? projectAssociation = null
    )
    {
        ValidateTeamCreation(name, teamManagerId, memberIds, existingTeams);
        var team = new Team(Guid.NewGuid(), name, teamManagerId, memberIds);
        team.Activate();
        // Only associate a project if there is an active associated project
        if (!activeAssociatedProject)
            return team;
        if (projectAssociation != null)
            team.IsAnyProjectAssociated(projectAssociation);

        return team;
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

        if (_memberIds.Count < 2 || TeamManagerId == null)
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

    public void AddMember(Guid memberId)
    {
        if (_memberIds.Contains(memberId))
            throw new DomainException("Member already exists in the team.");

        _memberIds.Add(memberId);
    }

    public void ChangeTeamManager(Guid newTeamManagerId)
    {
        if (newTeamManagerId == Guid.Empty)
            throw new DomainException("New team manager ID cannot be empty.");
        if (_memberIds.Contains(newTeamManagerId))
            throw new DomainException("New team manager must be a member of the team.");
        TeamManagerId = newTeamManagerId;
    }

    public void DeleteTeamMemberSafely(Guid memberId)
    {
        if (memberId == TeamManagerId)
            throw new DomainException("Cannot remove the team manager from the team.");

        if (!_memberIds.Contains(memberId))
            throw new DomainException("Member not found in the team.");
        _memberIds.Remove(memberId);
    }

    public void UpdateTeam(string name, Guid teamManagerId, List<Guid> memberIds)
    {
        if (this.Name == name && this._memberIds.SequenceEqual(memberIds))
            throw new DomainException("No changes detected in the team details.");

        this.Name = name;
        this.TeamManagerId = teamManagerId;
        _memberIds.Clear();
        _memberIds.AddRange(memberIds);
    }

    public (bool, Message?) CanMemberJoinNewTeam(TransfertMemberDto transfertMemberDto)
    {
        if (_memberIds == null || _memberIds.Count == 0)
            return (true, null); // Le membre n'existe pas dans une équipe
        if (!transfertMemberDto.AffectationStatus.IsTransferAllowed)
            return (
                false,
                new Message
                {
                    Status = 400,
                    Detail =
                        $"The team member {transfertMemberDto.MemberTeamIdDto} cannot be added in a new team.",
                    Type = "Business Rule Violation",
                    Title = "Not allow member",
                }
            );
        if (transfertMemberDto.AffectationStatus.LeaveDate.AddDays(7) > DateTime.UtcNow)
            return (
                false,
                new Message
                {
                    Type = "Business Rule Violation",
                    Title = "Member Cooldown Period",
                    Detail =
                        $"member {transfertMemberDto!.MemberTeamIdDto} must wait 7 days before being added to a new team.",
                    Status = 400,
                }
            ); // Moins de 7 jours : refus
        return (true, null);
    }
}
