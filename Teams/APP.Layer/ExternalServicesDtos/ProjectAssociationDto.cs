using Newtonsoft.Json;

namespace Teams.APP.Layer.ExternalServicesDtos;

public record ProjectAssociationDto(
    [property: JsonProperty(Required = Required.Always)] Guid TeamManagerIdDto,
    [property: JsonProperty(Required = Required.Always)] string TeamNameDto,
    [property: JsonProperty(Required = Required.Always)] DateTime ProjectStartDateDto
);
