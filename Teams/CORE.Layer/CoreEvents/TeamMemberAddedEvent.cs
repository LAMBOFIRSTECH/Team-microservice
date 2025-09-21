namespace Teams.CORE.Layer.CoreEvents;

public class TeamMemberAddedEvent : IDomainEvent
{
    public Guid TeamId { get; }
    public Guid MemberId { get; }
    public TeamMemberAddedEvent(Guid teamId, Guid memberId)
    {
        TeamId = teamId;
        MemberId = memberId;

    }
}
