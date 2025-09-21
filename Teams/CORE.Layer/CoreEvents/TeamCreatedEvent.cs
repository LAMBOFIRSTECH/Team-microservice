namespace Teams.CORE.Layer.CoreEvents;
public class TeamCreatedEvent : IDomainEvent
{
    public Guid TeamId { get; }
    public TeamCreatedEvent(Guid teamId) => TeamId = teamId;
}
