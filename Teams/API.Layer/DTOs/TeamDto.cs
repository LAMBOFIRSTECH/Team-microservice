using System.ComponentModel.DataAnnotations;

namespace Teams.API.Layer.DTOs;
public class TeamDto
{
    [Display(Name = "Team ID")]
    public Guid Id { get; set; }
    public string? Name { get; set; }
}
