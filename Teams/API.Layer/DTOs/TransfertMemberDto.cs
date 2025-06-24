using Newtonsoft.Json;

namespace Teams.API.Layer.DTOs
{
    // Pourquoi c'est un record ?? adapté pour les DTOs
    // car immuable et facile à sérialiser/désérialiser.
    public record AffectationStatus(
        [property: JsonProperty(Required = Required.Always)] bool IsTransferAllowed,
        [property: JsonProperty(Required = Required.Always)] DateTime LeaveDate
    );

    public record TransfertMemberDto(
        [property: JsonProperty(Required = Required.Always)] Guid MemberId,
        [property: JsonProperty(Required = Required.Always)] string SourceTeam,
        [property: JsonProperty(Required = Required.Always)] string DestinationTeam,
        [property: JsonProperty(Required = Required.Always)] AffectationStatus AffectationStatus
    );
}
