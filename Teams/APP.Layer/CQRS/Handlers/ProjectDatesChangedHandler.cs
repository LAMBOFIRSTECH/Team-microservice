using MediatR;
using Teams.APP.Layer.EventNotification;
using Teams.APP.Layer.Interfaces;
using Teams.CORE.Layer.CoreEvents;

namespace Teams.APP.Layer.CQRS.Handlers;

public class ProjectDatesChangedHandler(IProjectExpiryScheduler _scheduler)
    : INotificationHandler<DomainEventNotification<ProjectDatesChangedEvent>>
{
    public async Task Handle(
        DomainEventNotification<ProjectDatesChangedEvent> notification,
        CancellationToken ct
    )
    {
        // Une date a changé → on recalcule la prochaine échéance et on reprogramme le timer
        await _scheduler.RescheduleAsync(ct);
    }
}
