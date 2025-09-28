using MediatR;
using Teams.APP.Layer.EventNotification;
using Teams.APP.Layer.Helpers;
using Teams.APP.Layer.Interfaces;
using Teams.CORE.Layer.CoreEvents;
using Teams.CORE.Layer.Entities;
using Teams.CORE.Layer.Interfaces;

namespace Teams.APP.Layer.DomainHandlers;

public class TeamDomainHandler(
    IProjectExpirySchedule _projectScheduler,
    ITeamLifecycleScheduler _teamLifecycleScheduler,
    IServiceScopeFactory _scopeFactory,
    ILogger<TeamDomainHandler> _log
)
    : INotificationHandler<DomainEventNotification<TeamCreatedEvent>>,
        INotificationHandler<DomainEventNotification<ProjectDateChangedEvent>>
{

    public async Task Handle(
        DomainEventNotification<TeamCreatedEvent> notification,
        CancellationToken ct = default
    )
    {
        _log.LogWarning("🔥 TeamCreatedEvent handler triggered for TeamId {Id}", notification.DomainEvent.TeamId);
        _log.LogInformation("🔄 TeamCreatedEvent received, rescheduling...");
        await _teamLifecycleScheduler.RescheduleAsync(ct);

        await Task.Delay(500);
    }
    private async Task<Team?> GetTeamByIdAsync(Guid teamId, CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ITeamRepository>();
        var team = await repo.GetTeamByIdAsync(teamId, ct);
        if (team == null)
        {
            _log.LogWarning("⚠️ Team with ID {TeamId} not found.", teamId);
        }
        return team;
    }

    public async Task Handle(
        DomainEventNotification<ProjectDateChangedEvent> notification,
        CancellationToken ct = default
    )
    {
        _log.LogWarning("🔥 ProjectDateChangedEvent handler triggered for TeamId {Id}", notification.DomainEvent.TeamId);
        LogHelper.Info("🔄 ProjectDateChangedEvent received, rescheduling...", _log);
        await _projectScheduler.RescheduleAsync(ct);
    }
}
