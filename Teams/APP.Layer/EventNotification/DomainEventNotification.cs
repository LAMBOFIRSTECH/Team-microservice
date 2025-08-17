using MediatR;
using Teams.CORE.Layer.CoreEvents;

namespace Teams.APP.Layer.EventNotification;

public class DomainEventNotification<TDomainEvent> : INotification
    where TDomainEvent : IDomainEvent
{
    public TDomainEvent DomainEvent { get; }

    public DomainEventNotification(TDomainEvent domainEvent) => DomainEvent = domainEvent;
}
