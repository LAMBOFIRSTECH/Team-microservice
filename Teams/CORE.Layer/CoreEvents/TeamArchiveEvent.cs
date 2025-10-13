using NodaTime;
using Teams.CORE.Layer.CoreInterfaces;
namespace Teams.CORE.Layer.CoreEvents;

public record TeamArchiveEvent(Guid TeamId, string TeamName, Instant ArchivedAt, Guid EventId) : IDomainEvent;
