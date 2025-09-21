namespace Teams.CORE.Layer.CoreEvents;

public class TeamMemberRemoveEvent : IDomainEvent
{
    public Guid TeamId { get; }
    public Guid MemberId { get; }

    public TeamMemberRemoveEvent(Guid teamId, Guid memberId)
    {
        TeamId = teamId;
        MemberId = memberId;

    }
}
