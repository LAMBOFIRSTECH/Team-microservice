using Newtonsoft.Json;

namespace Teams.APP.Layer.DTOs;

public record DeleteTeamMemberDto(
    [property: JsonProperty(Required = Required.Always)] Guid MemberId,
    [property: JsonProperty(Required = Required.Always)] string? TeamName
);
