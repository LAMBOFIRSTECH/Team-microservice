namespace Teams.CORE.Layer.CoreInterfaces;
public interface IDomainEventHandler<TEvent> where TEvent : IDomainEvent
{
    Task Handle(TEvent @event, CancellationToken ct);
}
