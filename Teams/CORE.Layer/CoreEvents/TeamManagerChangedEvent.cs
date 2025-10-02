using Teams.CORE.Layer.CoreInterfaces;
namespace Teams.CORE.Layer.CoreEvents;

public record TeamManagerChangedEvent(Guid teamId) : IDomainEvent;