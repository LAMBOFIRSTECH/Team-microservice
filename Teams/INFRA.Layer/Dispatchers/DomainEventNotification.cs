using MediatR;
using Teams.CORE.Layer.CoreInterfaces;

namespace Teams.INFRA.Layer.Dispatchers;

// Ce wrapper est le seul à connaître MediatR ET ton Event
public class DomainEventNotification<TEvent>(TEvent domainEvent) : INotification 
    where TEvent : IDomainEvent
{
    public TEvent DomainEvent { get; } = domainEvent;
}