namespace Teams.CORE.Layer.CoreEvents;
public class TeamMaturityEvent : IDomainEvent
{
    public Guid TeamId { get; }
    public TeamMaturityEvent(Guid teamId) => TeamId = teamId;
}
