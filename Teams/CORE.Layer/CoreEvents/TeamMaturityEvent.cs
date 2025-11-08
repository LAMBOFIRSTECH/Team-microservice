using Teams.CORE.Layer.CoreInterfaces;
namespace Teams.CORE.Layer.CoreEvents;

public record TeamMaturityEvent(Guid teamId) : IDomainEvent
{ }