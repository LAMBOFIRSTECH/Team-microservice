using MediatR;
using Teams.CORE.Layer.CoreInterfaces;
// C'est un wrapper ici
namespace Teams.APP.Layer.WrapperEventToNotification;

public class DomainEventNotification<TDomainEvent> : INotification
    where TDomainEvent : IDomainEvent
{
    public TDomainEvent DomainEvent { get; }
    public DomainEventNotification(TDomainEvent _domainEvent) => DomainEvent = _domainEvent;
}
