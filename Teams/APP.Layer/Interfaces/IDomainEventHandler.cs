using Teams.CORE.Layer.CoreInterfaces;

namespace Teams.APP.Layer.Interfaces;
public interface IDomainEventHandler<TEvent> where TEvent : IDomainEvent
{
    Task Handle(TEvent @event, CancellationToken ct);
}
