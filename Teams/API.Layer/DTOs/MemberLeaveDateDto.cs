using Newtonsoft.Json;

namespace Teams.API.Layer.DTOs;

// Pourquoi c'est un record ??
public record MemberLeaveDateDto(
    [property: JsonProperty(Required = Required.Always)] Guid MemberId,
    [property: JsonProperty(Required = Required.Always)] string TeamName,
    [property: JsonProperty(Required = Required.Always)] DateTime LastLeaveDate
);
