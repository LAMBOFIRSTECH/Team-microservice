using MediatR;
using Teams.CORE.CoreInterfaces;

namespace Teams.INFRA.Dispatchers;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default);
}
public class DomainEventDispatcher(IMediator _mediator) : IDomainEventDispatcher
{
    public async Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        foreach (var @event in events)
        {
            // On emballe l'événement du Core dans le wrapper de l'Infra
            var genericType = typeof(DomainEventNotification<>).MakeGenericType(@event.GetType());
            var notification = Activator.CreateInstance(genericType, @event) as INotification;

            if (notification != null)
            {
                await _mediator.Publish(notification, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
