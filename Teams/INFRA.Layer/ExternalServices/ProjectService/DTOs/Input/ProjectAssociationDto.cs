using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Teams.CORE.Layer.Entities.TeamAggregate.InternalEntities;

namespace Teams.INFRA.Layer.ExternalServices.ProjectService.DTOs.Input;
public record ProjectStateDto(
    [property: JsonProperty(Required = Required.Always)]
    [property: JsonConverter(typeof(StringEnumConverter))]
        VoState State
);

public record DetailDto(
    [property: JsonProperty(Required = Required.Always)] string ProjectName,
    [property: JsonProperty(Required = Required.Always)] DateTimeOffset ProjectStartDate,
    [property: JsonProperty(Required = Required.Always)] DateTimeOffset ProjectEndDate,
    [property: JsonProperty(Required = Required.Always)] ProjectStateDto VoState
);

/// <summary>
/// C'est le contrat d'intégration (DTO "sale") que le service de projet externe envoie à notre application.
/// </summary>
/// <param name="ProjectId"></param>
/// <param name="TeamManagerId"></param>
/// <param name="TeamName"></param>
/// <param name="Details"></param>
public record ProjectAssociationDto(
    [property: JsonProperty(Required = Required.Always)] Guid ProjectId,
    [property: JsonProperty(Required = Required.Always)] Guid TeamManagerId,
    [property: JsonProperty(Required = Required.Always)] string TeamName,
    [property: JsonProperty(Required = Required.Always)] List<DetailDto> Details
);
