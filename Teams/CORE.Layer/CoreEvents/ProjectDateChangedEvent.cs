using Teams.CORE.Layer.CoreInterfaces;
namespace Teams.CORE.Layer.CoreEvents;

public record ProjectDateChangedEvent(Guid teamId)  : IDomainEvent
{

}