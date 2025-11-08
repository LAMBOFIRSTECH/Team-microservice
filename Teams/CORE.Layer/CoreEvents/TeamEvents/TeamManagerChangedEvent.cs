using Teams.CORE.Layer.CoreInterfaces;
namespace Teams.CORE.Layer.CoreEvents.TeamEvents;

public record TeamManagerChangedEvent(Guid teamId) : IDomainEvent { }