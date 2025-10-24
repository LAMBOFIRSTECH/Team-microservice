using Teams.CORE.Layer.CoreInterfaces;
namespace Teams.CORE.Layer.Entities;

public abstract class AggregateEntity : IHasDomainEvents
{
   public Guid Id { get; protected set; }
   private readonly List<IDomainEvent> _domainEvents = new();
   public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
   public void AddDomainEvent(IDomainEvent @event) => _domainEvents.Add(@event);
   public void ClearDomainEvents() => _domainEvents.Clear();
   public override bool Equals(object? obj)
   {
      if (obj is not AggregateEntity other) return false;
      if (ReferenceEquals(this, other)) return true;
      return Id == other.Id;
   }
   public override int GetHashCode() => Id.GetHashCode();
}
