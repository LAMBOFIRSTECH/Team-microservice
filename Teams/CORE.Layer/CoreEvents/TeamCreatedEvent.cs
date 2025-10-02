using Teams.CORE.Layer.CoreInterfaces;
namespace Teams.CORE.Layer.CoreEvents;

public record TeamCreatedEvent(Guid teamId) : IDomainEvent;