using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Teams.CORE.Layer.ValueObjects;

namespace Teams.INFRA.Layer.ExternalServicesDtos;

public record ProjectStateDto(
    [property: JsonProperty(Required = Required.Always)]
    [property: JsonConverter(typeof(StringEnumConverter))]
        ProjectState State
);

public record ProjectAssociationDto(
    [property: JsonProperty(Required = Required.Always)] Guid TeamManagerId,
    [property: JsonProperty(Required = Required.Always)] string TeamName,
    [property: JsonProperty(Required = Required.Always)] string ProjectName,
    [property: JsonProperty(Required = Required.Always)] DateTime ProjectStartDate,
    [property: JsonProperty(Required = Required.Always)] DateTime ProjectEndDate,
    [property: JsonProperty(Required = Required.Always)] ProjectStateDto ProjectState
);
