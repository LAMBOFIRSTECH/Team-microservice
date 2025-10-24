namespace Teams.API.Layer.DTOs;

public class TeamRequestDto
{
    public Guid Id { get; set; }
    public string? Name { get; }
    public Guid TeamManagerId { get; }
    public IEnumerable<Guid> MembersId { get; }

#pragma warning disable CS8618 
    public TeamRequestDto() { }
#pragma warning restore CS8618 


    public TeamRequestDto(
        Guid identifier,
        Guid managerId,
        string teamName,
        IEnumerable<Guid> memberIds,
        bool includeMembers = false
    )
    {
        Id = identifier;
        TeamManagerId = managerId;
        Name = teamName;
        if (includeMembers)
            MembersId = memberIds ?? [];
        else
            MembersId = [];
    }
}
