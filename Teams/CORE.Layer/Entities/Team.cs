using System.ComponentModel.DataAnnotations;

namespace Teams.CORE.Layer.Entities;
public class Team
{
    [Required]
    [Key]
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public Guid TeamManagerId { get; set; }
    public List<Guid> MemberId { get; set; } = new();
}