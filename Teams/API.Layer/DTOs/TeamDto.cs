namespace Teams.API.Layer.DTOs;

public class TeamDto
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public Guid TeamManagerId { get; init; }
    public IEnumerable<Guid> MembersIds { get; set; } = Array.Empty<Guid>();
    /// <summary>
    /// Pour EF Core et la désérialisation | n'existerait pas si toutes les propriétés étaient init (non modifiable après initialisation de l'objet)
    /// </summary>
    public TeamDto() { }

    public TeamDto(Guid managerId, string teamName, IEnumerable<Guid> memberIds, bool includeMembers = false)
    {
        TeamManagerId = managerId;
        Name = teamName;
        MembersIds = includeMembers ? memberIds ?? Array.Empty<Guid>() : Array.Empty<Guid>();
    }
}
