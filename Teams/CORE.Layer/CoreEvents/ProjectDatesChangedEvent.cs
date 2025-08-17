// using MediatR;

namespace Teams.CORE.Layer.CoreEvents;

public class ProjectDatesChangedEvent : IDomainEvent
{
    public Guid TeamId { get; }

    public ProjectDatesChangedEvent(Guid teamId) => TeamId = teamId;
}
