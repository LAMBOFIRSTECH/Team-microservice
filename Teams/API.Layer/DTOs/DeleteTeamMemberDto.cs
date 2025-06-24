namespace Teams.API.Layer.DTOs;

public class DeleteTeamMemberDto
{
    public Guid MemberId { get; set; }
    public string? TeamName { get; set; }
}
