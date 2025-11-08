using Teams.CORE.Layer.CoreInterfaces;
namespace Teams.CORE.Layer.CoreEvents;
public record TeamArchiveEvent(Guid TeamId, string TeamName, DateTimeOffset ArchivedAt, Guid EventId) : IDomainEvent
{
}
