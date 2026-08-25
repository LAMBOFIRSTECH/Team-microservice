using MediatR;
using Teams.CORE.CoreInterfaces;
namespace Teams.INFRA.Dispatchers;

// Ce wrapper est le seul à connaître MediatR ET ton Event
public class DomainEventNotification<TEvent>(TEvent domainEvent) : INotification where TEvent : IDomainEvent
{
    public TEvent DomainEvent { get; } = domainEvent;
}