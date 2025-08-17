using MediatR;
using Teams.APP.Layer.EventNotification;
using Teams.APP.Layer.Interfaces;
using Teams.CORE.Layer.CoreEvents;

namespace Teams.APP.Layer.CQRS.Handlers;

public class ProjectDatesChangedHandler
    : INotificationHandler<DomainEventNotification<ProjectDatesChangedEvent>>
{
    private readonly IProjectExpiryScheduler _scheduler;

    public ProjectDatesChangedHandler(IProjectExpiryScheduler scheduler) => _scheduler = scheduler;

    public async Task Handle(
        DomainEventNotification<ProjectDatesChangedEvent> notification,
        CancellationToken ct
    )
    {
        // Une date a changé → on recalcule la prochaine échéance et on reprogramme le timer
        await _scheduler.RescheduleAsync(ct);
    }
}
