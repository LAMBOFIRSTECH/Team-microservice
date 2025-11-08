using NodaTime;
using Teams.CORE.Layer.CoreInterfaces;

namespace Teams.CORE.Layer.CoreEvents.TeamEvents;
public record TeamDeletedEvent(Guid TeamId, string TeamName, DateTimeOffset DeletionDate, Guid EventId)  : IDomainEvent
{

}
