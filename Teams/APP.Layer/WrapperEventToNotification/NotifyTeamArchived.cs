using MediatR;
using Teams.CORE.Layer.CoreInterfaces;

namespace Teams.APP.Layer.EventNotification;

// faux tres faux meme
public class NotifyTeamArchived<TDomainEvent> : INotification where TDomainEvent : IDomainEvent
{
    public TDomainEvent DomainEvent { get; }

    public NotifyTeamArchived(TDomainEvent _domainEvent) => DomainEvent = _domainEvent;
}
