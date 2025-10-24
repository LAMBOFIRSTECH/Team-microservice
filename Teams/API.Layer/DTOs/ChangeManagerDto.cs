namespace Teams.API.Layer.DTOs;

public class ChangeManagerDto
{
    public string? Name { get; set; }
    public Guid OldTeamManagerId { get; set; }
    public Guid NewTeamManagerId { get; set; }

    public ChangeManagerDto() { }

    public ChangeManagerDto(string name, string oldTeamManagerId, string newTeamManagerId)
    {
        Name = name;
        OldTeamManagerId = Guid.TryParse(oldTeamManagerId, out var oldId) ? oldId : Guid.Empty;
        NewTeamManagerId = Guid.TryParse(newTeamManagerId, out var newId) ? newId : Guid.Empty;
    }

    public ChangeManagerDto(string name, Guid oldTeamManagerId, Guid newTeamManagerId)
    {
        Name = name;
        OldTeamManagerId = oldTeamManagerId;
        NewTeamManagerId = newTeamManagerId;
    }
}
