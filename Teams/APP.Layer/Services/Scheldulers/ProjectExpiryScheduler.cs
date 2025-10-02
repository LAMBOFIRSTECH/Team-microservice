using Teams.APP.Layer.Helpers;
using Teams.APP.Layer.Interfaces;
using Teams.CORE.Layer.CoreInterfaces;

namespace Teams.APP.Layer.Scheldulers.Services;

public class ProjectExpiryScheduler(
    IServiceScopeFactory _scopeFactory,
    ILogger<ProjectExpiryScheduler> _log
) : IHostedService, IDisposable, IProjectExpirySchedule
{
    private Timer? _timer;
    private DateTime? _nextProjectDateExpiration;

    public async Task StartAsync(CancellationToken ct)
    {
        LogHelper.Info("🚀 ProjectExpiryChecker starting...", _log);
        await ScheduleNextCheckAsync(ct);
    }

    public Task StopAsync(CancellationToken ct)
    {
        LogHelper.Info("🛑 ProjectExpiryChecker stopping timer...", _log);
        _timer?.Change(Timeout.Infinite, 0);
        _timer = null;
        return Task.CompletedTask;
    }

    public void Dispose() => _timer?.Dispose();
    public async Task RescheduleAsync(CancellationToken ct) => await ScheduleNextCheckAsync(ct);
    private async Task CheckExpiredProjects(CancellationToken ct = default)
    {
        LogHelper.Info($"⏱ Running CheckExpiredProjects at {DateTime.Now}", _log);

        using var scope = _scopeFactory.CreateScope();
        var teamRepository = scope.ServiceProvider.GetRequiredService<ITeamRepository>();

        var expiredTeams = await teamRepository.GetTeamsWithExpiredProject(ct);
        foreach (var team in expiredTeams)
        {
            team.RemoveExpiredProjects();
            await teamRepository.UpdateTeamAsync(team, ct);
            LogHelper.Info($"✅ Project has been dissociated from team {team.Name}", _log);
        }
        await ScheduleNextCheckAsync(ct);
    }
    private async Task ScheduleNextCheckAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var teamRepository = scope.ServiceProvider.GetRequiredService<ITeamRepository>();
        _nextProjectDateExpiration = await teamRepository.GetNextProjectExpirationDate(ct);

        if (_nextProjectDateExpiration is null)
        {
            LogHelper.Info("⏸ No future deadlines found. Timer stopped.", _log);
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            _timer = null;
            return;
        }
        var now = DateTime.Now;
        var delay = _nextProjectDateExpiration.Value - now;
        if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;
        LogHelper.Info($"▶️ Next check scheduled for {_nextProjectDateExpiration}", _log);

        _timer?.Dispose();
        _timer = new Timer(
            async _ =>
            {
                try
                {
                    await CheckExpiredProjects();
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "❌ Error while checking expired projects");
                }
            },
            null,
            delay,
            Timeout.InfiniteTimeSpan // one-shot → will reschedule after execution
        );
    }

}
