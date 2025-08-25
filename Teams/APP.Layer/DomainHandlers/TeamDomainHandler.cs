using MediatR;
using Teams.APP.Layer.EventNotification;
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
    private async Task<Team?> GetTeamByIdAsync(Guid teamId, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ITeamRepository>();
        var team = await repo.GetTeamByIdAsync(teamId);
        if (team == null)
        {
            _log.LogWarning("⚠️ Team with ID {TeamId} not found.", teamId);
        }
        return team;
    }

    public async Task Handle(
        DomainEventNotification<TeamCreatedEvent> notification,
        CancellationToken ct
    )
    {
        _log.LogInformation("🔄 TeamCreatedEvent received, rescheduling...");

        var team = await GetTeamByIdAsync(notification.DomainEvent.TeamId, ct);
        await _teamScheduler.RescheduleAsync(team!, ct);
    }

    public async Task Handle(
        DomainEventNotification<TeamArchiveEvent> notification,
        CancellationToken ct
    )
    {
        _log.LogInformation(
            $"🔄 TeamArchivedEvent received for Team {notification.DomainEvent.TeamId}, rescheduling..."
        );

        var team = await GetTeamByIdAsync(notification.DomainEvent.TeamId, ct);
        await _teamScheduler.RescheduleAsync(team!, ct);
    }

    public async Task Handle(
        DomainEventNotification<TeamMaturityEvent> notification,
        CancellationToken ct
    )
    {
        _log.LogInformation(
            $"🔄 TeamMaturityEvent received for Team {notification.DomainEvent.TeamId}, rescheduling maturity check..."
        );
        await _maturitySchedule.RescheduleMaturityTeamAsync(ct);
    }

    public async Task Handle(
        DomainEventNotification<ProjectDateChangedEvent> notification,
        CancellationToken ct
    )
    {
        _log.LogInformation("🔄 ProjectDateChangedEvent received, rescheduling...");
        await _projectScheduler.RescheduleAsync(ct);
    }
}
