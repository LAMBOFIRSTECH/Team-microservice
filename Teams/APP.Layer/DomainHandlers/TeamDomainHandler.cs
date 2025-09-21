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
    ITeamExpiryScheduler _teamScheduler,
    ITeamMaturityScheduler _maturitySchedule,
    IServiceScopeFactory _scopeFactory,
    ILogger<TeamDomainHandler> _log
)
    : INotificationHandler<DomainEventNotification<TeamCreatedEvent>>,
        INotificationHandler<DomainEventNotification<TeamArchiveEvent>>,
        INotificationHandler<DomainEventNotification<TeamMaturityEvent>>,
        INotificationHandler<DomainEventNotification<ProjectDateChangedEvent>>
{
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
        DomainEventNotification<TeamCreatedEvent> notification,
        CancellationToken ct = default
    )
    {
        _log.LogInformation("🔄 TeamCreatedEvent received, rescheduling...");
        await _teamScheduler.RescheduleAsync(ct);
    }

    public async Task Handle(
        DomainEventNotification<TeamArchiveEvent> notification,
        CancellationToken ct = default
    )
    {
        LogHelper.Info(
            $"🔄 TeamArchivedEvent received for Team Id {notification.DomainEvent.TeamId}, rescheduling...",
            _log
        );
        await _teamScheduler.RescheduleAsync(ct);
    }

    public async Task Handle(
        DomainEventNotification<TeamMaturityEvent> notification,
        CancellationToken ct = default
    )
    {
        LogHelper.Info(
            $"🔄 TeamMaturityEvent received for Team Id {notification.DomainEvent.TeamId}, rescheduling maturity check...",
            _log
        );
        await _maturitySchedule.RescheduleMaturityTeamAsync(ct);
    }

    public async Task Handle(
        DomainEventNotification<ProjectDateChangedEvent> notification,
        CancellationToken ct = default
    )
    {
        LogHelper.Info("🔄 ProjectDateChangedEvent received, rescheduling...", _log);
        await _projectScheduler.RescheduleAsync(ct);
    }
}
