namespace Teams.API.Layer.DTOs;

public class TeamRequestDto
{
    public Guid Id { get; }
    public string? Name { get; }
    public Guid TeamManagerId { get; }
    public List<Guid> MemberId { get; } = new();

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
