using MediatR;
using Teams.APP.Layer.WrapperEventToNotification;
using Teams.APP.Layer.Helpers;
using Teams.APP.Layer.Interfaces;
using  Teams.CORE.Layer.CoreEvents.TeamEvents;

namespace Teams.APP.Layer.DomainHandlers;
public class TeamDomainHandler(
    IProjectExpirySchedule _projectScheduler,
    ITeamExpiryScheduler _teamExpiryScheduler,
    ILogger<TeamDomainHandler> _log
) : INotificationHandler<DomainEventNotification<TeamCreatedEvent>>,
    INotificationHandler<DomainEventNotification<ProjectDateChangedEvent>>
{ // revoir ce handler urgemment

    public async Task Handle(DomainEventNotification<TeamCreatedEvent> notification, CancellationToken ct = default)
    {
        _log.LogWarning("🔥 TeamCreatedEvent handler triggered for TeamId {Id}", notification.DomainEvent.TeamId);
        await _teamExpiryScheduler.RescheduleAsync(ct);
        _log.LogInformation("🔄 TeamCreatedEvent received, rescheduling...");
    }

    public async Task Handle(DomainEventNotification<ProjectDateChangedEvent> notification, CancellationToken ct = default)
    {
        _log.LogWarning("🔥 ProjectDateChangedEvent handler triggered for TeamId {Id}", notification.DomainEvent.teamId);
        await _projectScheduler.RescheduleAsync(ct);
        LogHelper.Info("🔄 ProjectDateChangedEvent received, rescheduling...", _log);
    }
}
