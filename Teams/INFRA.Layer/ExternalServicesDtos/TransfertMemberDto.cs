using Newtonsoft.Json;

namespace Teams.INFRA.Layer.ExternalServicesDtos;
public record AffectationStatus(
    [property: JsonProperty(Required = Required.Always)] bool IsTransferAllowed,
    [property: JsonProperty(Required = Required.Always)] string ContratType,
    [property: JsonProperty(Required = Required.Always)] DateTime LeaveDate
);

public record TransfertMemberDto(
    [property: JsonProperty(Required = Required.Always)] Guid MemberTeamId,
    [property: JsonProperty(Required = Required.Always)] string SourceTeam,
    [property: JsonProperty(Required = Required.Always)] string DestinationTeam,
    [property: JsonProperty(Required = Required.Always)] AffectationStatus AffectationStatus
);
