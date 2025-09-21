namespace Teams.API.Layer.DTOs;

public class TeamDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public Guid TeamManagerId { get; set; }
    public IEnumerable<Guid> MembersId { get; set; }

#pragma warning disable CS8618 
    public TeamDto() { }
#pragma warning restore CS8618


    public TeamDto(
        Guid managerId,
        string teamName,
        IEnumerable<Guid> memberIds,
        bool includeMembers = false
    )
    {
        TeamManagerId = managerId;
        Name = teamName;
        if (includeMembers)
            MembersId = memberIds ?? [];
        else
            MembersId = [];
    }
}
