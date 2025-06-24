namespace Teams.API.Layer.DTOs;

public class TeamRequestDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public Guid TeamManagerId { get; set; }
    public List<Guid> MemberId { get; set; } = new();

    public TeamRequestDto() { }

    public TeamRequestDto(
        Guid identifier,
        Guid managerId,
        string teamName,
        bool includeMembers = false,
        List<Guid>? memberIds = null
    )
    {
        Id = identifier;
        TeamManagerId = managerId;
        Name = teamName;
        if (includeMembers)
            MemberId = memberIds ?? [];
        else
            MemberId = [];
    }
}
