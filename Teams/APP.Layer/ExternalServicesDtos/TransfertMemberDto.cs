using Newtonsoft.Json;

namespace Teams.APP.Layer.ExternalServicesDtos;

// Pourquoi c'est un record ?? adapté pour les DTOs
// car immuable et facile à sérialiser/désérialiser.
public record AffectationStatus(
    [property: JsonProperty(Required = Required.Always)] bool IsTransferAllowed,
    [property: JsonProperty(Required = Required.Always)] DateTime LeaveDate
);

public record TransfertMemberDto(
    [property: JsonProperty(Required = Required.Always)] Guid MemberTeamIdDto,
    [property: JsonProperty(Required = Required.Always)] string SourceTeamDto,
    [property: JsonProperty(Required = Required.Always)] string DestinationTeamDto,
    [property: JsonProperty(Required = Required.Always)] AffectationStatus AffectationStatus
);
