using Teams.CORE.Layer.CoreInterfaces;
namespace Teams.CORE.Layer.CoreEvents.TeamEvents;

public record TeamMaturityEvent(Guid teamId) : IDomainEvent
{ }