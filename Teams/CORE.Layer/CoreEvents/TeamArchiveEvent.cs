using System;

namespace Teams.CORE.Layer.CoreEvents;

public class TeamArchiveEvent : IDomainEvent
{
    public Guid TeamId { get; }

    public TeamArchiveEvent(Guid teamId) => TeamId = teamId;
}
