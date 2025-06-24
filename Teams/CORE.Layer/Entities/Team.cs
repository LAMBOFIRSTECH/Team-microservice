using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Teams.CORE.Layer.BusinessExceptions;

namespace Teams.CORE.Layer.Entities;

public class Team
{
    [Key]
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid TeamManagerId { get; set; }
    public string? MemberIdSerialized { get; set; } = string.Empty;
    public List<Guid> MemberId
    {
        get =>
            string.IsNullOrEmpty(MemberIdSerialized)
                ? new List<Guid>()
                : JsonConvert.DeserializeObject<List<Guid>>(MemberIdSerialized) ?? new List<Guid>();
        set => MemberIdSerialized = JsonConvert.SerializeObject(value);
    }

    public void RemoveMember(Guid memberId)
    {
        var members = MemberId;
        members.RemoveAll(m => m == memberId);
        MemberId = members;
    }

    public void AddMember(Guid memberId)
    {
        var members = MemberId;
        members.Add(memberId);
        MemberId = members;
    }

    public static Team Create(
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

        var uniqueMemberIds = memberIds.Distinct().ToList();
        if (uniqueMemberIds.Count != memberIds.Count)
            throw new DomainException("Team members must be unique.");

        if (!uniqueMemberIds.Contains(teamManagerId))
            throw new DomainException("The team manager must be one of the team members.");

        if (existingTeams.Any(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException($"A team with the name '{name}' already exists.");

        return new Team
        {
            Id = Guid.NewGuid(),
            Name = name,
            TeamManagerId = teamManagerId,
            MemberId = uniqueMemberIds,
        };
    }

    public void DeleteTeamMemberSafely(Guid memberId)
    {
        if (memberId == TeamManagerId)
            throw new DomainException("Cannot remove the team manager from the team.");
        var members = MemberId;
        if (!members.Contains(memberId))
            throw new DomainException("Member not found in the team.");
        members.Remove(memberId);
        MemberId = members;
    }

    public void UpdateTeam(string name, Guid teamManagerId, List<Guid> memberIds)
    {
        if (this.Name == name && this.MemberId.SequenceEqual(memberIds))
            throw new DomainException("No changes detected in the team details.");

        this.Name = name;
        this.TeamManagerId = teamManagerId;
        this.MemberId = memberIds;
    }
}
