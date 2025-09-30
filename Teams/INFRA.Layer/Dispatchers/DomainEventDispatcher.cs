using MediatR;
using Teams.APP.Layer.EventNotification;
using Teams.CORE.Layer.CoreEvents;

namespace Teams.INFRA.Layer.Dispatchers;
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default);
}

public class DomainEventDispatcher(IMediator _mediator, ILogger<DomainEventDispatcher> _log) : IDomainEventDispatcher
{
    public async Task DispatchAsync(
        IEnumerable<IDomainEvent> events,
        CancellationToken ct = default
    )
    {
        foreach (var @event in events)
        {
            var notifType = typeof(DomainEventNotification<>).MakeGenericType(@event.GetType());
            var notification = (INotification)Activator.CreateInstance(notifType, @event)!;
            await _mediator.Publish(notification, ct);
        }
        _log.LogInformation("Dispatching {Count} domain events...", events.Count());

    }
}
