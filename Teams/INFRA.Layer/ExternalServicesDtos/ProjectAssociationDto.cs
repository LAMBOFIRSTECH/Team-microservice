using Newtonsoft.Json;

namespace Teams.INFRA.Layer.ExternalServicesDtos;

public record ProjectState([property: JsonProperty(Required = Required.Always)] Enum State);

public record ProjectAssociationDto(
    [property: JsonProperty(Required = Required.Always)] Guid TeamManagerId,
    [property: JsonProperty(Required = Required.Always)] string TeamName,
    [property: JsonProperty(Required = Required.Always)] DateTime ProjectStartDate,
    [property: JsonProperty(Required = Required.Always)] ProjectState ProjectState
);
