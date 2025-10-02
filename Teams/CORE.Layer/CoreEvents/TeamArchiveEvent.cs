using Teams.CORE.Layer.CoreInterfaces;
namespace Teams.CORE.Layer.CoreEvents;

public record TeamArchiveEvent(Guid TeamId, string TeamName, DateTime ArchivedAt, Guid EventId) : IDomainEvent;
