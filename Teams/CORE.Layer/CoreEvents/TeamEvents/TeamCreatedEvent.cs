using Teams.CORE.Layer.CoreInterfaces;
namespace Teams.CORE.Layer.CoreEvents.TeamEvents;

public record TeamCreatedEvent(Guid TeamId, string TeamName, DateTimeOffset CreatedAt, Guid EventId) : IDomainEvent
{
   
}