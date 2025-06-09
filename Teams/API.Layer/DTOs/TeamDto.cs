namespace Teams.API.Layer.DTOs;

public class TeamDto
{
    public string? Name { get; set; }
    public Guid TeamManagerId { get; set; }
    public List<Guid> MemberId { get; set; } = new();
    public TeamDto()
    {

    }
    public TeamDto(Guid managerId, string teamName, bool includeMembers = false, List<Guid>? memberIds = null)
    {
        TeamManagerId = managerId;
        Name = teamName;
        if (includeMembers)
            MemberId = memberIds ?? [];
        else
            MemberId = [];

    }
}
