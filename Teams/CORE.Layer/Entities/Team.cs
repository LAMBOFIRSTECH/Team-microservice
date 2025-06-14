using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Teams.CORE.Layer.Entities;

public class Team
{
    [Key]
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid TeamManagerId { get; set; }
    public string? MemberIdSerialized { get; set; } = string.Empty;
    public List<Guid> MemberId
    {
        get =>
            string.IsNullOrEmpty(MemberIdSerialized)
                ? new List<Guid>()
                : JsonConvert.DeserializeObject<List<Guid>>(MemberIdSerialized) ?? new List<Guid>();
        set => MemberIdSerialized = JsonConvert.SerializeObject(value);
    }
}
