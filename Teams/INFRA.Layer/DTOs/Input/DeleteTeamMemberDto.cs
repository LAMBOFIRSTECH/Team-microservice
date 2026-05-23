using Newtonsoft.Json;
namespace Teams.INFRA.Layer.DTOs.Input;
public record DeleteTeamMemberDto([property: JsonProperty(Required = Required.Always)] Guid MemberId,[property: JsonProperty(Required = Required.Always)] string? TeamName);
