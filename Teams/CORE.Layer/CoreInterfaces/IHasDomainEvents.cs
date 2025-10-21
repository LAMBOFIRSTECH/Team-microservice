namespace Teams.CORE.Layer.CoreInterfaces;

public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void AddDomainEvent(IDomainEvent @event);
    void ClearDomainEvents();
}
