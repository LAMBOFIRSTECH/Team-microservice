using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Teams.INFRA.Messaging.DTOs;

/// C'est le contrat d'intégration (DTO "sale") que le service de projet externe envoie à notre application.
/// <summary>
/// Represents the association of a project with a team, including details about the project and its assignment state.
/// </summary>
public record ProjectAssociationDto
{
    [property: JsonProperty(Required = Required.Always)]
    public Guid ProjectId { get; init; }

    [property: JsonProperty(Required = Required.Always)]
    public Guid TeamManagerId { get; init; }

    [property: JsonProperty(Required = Required.Always)]
    public string TeamName { get; init; } = null!;

    [property: JsonProperty(Required = Required.Always)]
    public IReadOnlyCollection<DetailDto> Details { get; init; } = null!;

    [property: JsonProperty(Required = Required.Always)]
    [property: JsonConverter(typeof(StringEnumConverter))]
    public ExternalProjectAssignmentState AssignmentState { get; init; }

    /// <summary>
    /// Represents the state of a project in the context of an external service, indicating whether it is active or suspended.
    /// </summary>
    public enum ExternalProjectDetailState
    {
        Active = 0,
        Suspended = 1
    }

    /// <summary>
    /// Represents the assignment state of a project in the context of an external service, mirroring the internal ProjectAssignmentState enum but used for data transfer purposes.
    /// </summary>
    public enum ExternalProjectAssignmentState
    {
        /// <summary>
        /// The project is unassigned to any team.
        /// </summary>
        Unassigned = 0,
        /// <summary>
        /// The project is assigned to a team and is active in the project microservice.
        /// </summary>
        Assigned = 1,
        /// <summary>
        /// The project is suspended.
        /// </summary>
        Suspended = 2,
        /// <summary>
        /// The project is under review. signifies that the project is being evaluated for its current state and may require action from the team or management.
        /// </summary>
        UnderReview = 3,
        /// <summary>
        /// The project is unassigned after review.
        /// </summary>
        UnassignedAfterReview = 4
    }

    /// <summary>
    /// Represents the details of a project, including its name, start and end dates, current state, and suspension date if applicable.
    /// </summary>
    public record DetailDto(
        [property: JsonProperty(Required = Required.Always)] string ProjectName,
        [property: JsonProperty(Required = Required.Always)] DateTimeOffset ProjectStartDate,
        [property: JsonProperty(Required = Required.Always)] DateTimeOffset ProjectEndDate,
        [property: JsonProperty(Required = Required.Always)][property: JsonConverter(typeof(StringEnumConverter))] ExternalProjectDetailState State,
        [property: JsonProperty] DateTimeOffset? SuspendedAt
    );
}


