namespace Teams.CORE.Layer.CoreEvents;
public class TeamManagerChangedEvent : IDomainEvent
{
    public Guid TeamId { get; }
    public TeamManagerChangedEvent(Guid teamId) => TeamId = teamId;
}
