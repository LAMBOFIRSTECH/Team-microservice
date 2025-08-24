namespace Teams.CORE.Layer.CoreEvents;

public class ProjectDateChangedEvent : IDomainEvent
{
    public Guid TeamId { get; }

    public ProjectDateChangedEvent(Guid teamId) => TeamId = teamId;
}
