using NodaTime;
using Teams.APP.Layer.Helpers;
using Teams.APP.Layer.Interfaces;

namespace Teams.APP.Layer.Services.Scheldulers;

public class ProjectExpiryScheduler(
    IServiceScopeFactory _scopeFactory,
    ILogger<ProjectExpiryScheduler> _log
) : IHostedService, IDisposable, IProjectExpirySchedule
{
    private readonly object _lock = new();
    private Timer? _timer;

    public async Task StartAsync(CancellationToken ct)
    {
        LogHelper.Info("🚀 ProjectExpiryScheduler starting...", _log);
        await ScheduleNextCheckAsync(ct);
    }

    public Task StopAsync(CancellationToken ct)
    {
        LogHelper.Info("🛑 ProjectExpiryScheduler stopping timer...", _log);
        lock (_lock)
        {
            _timer?.Change(Timeout.Infinite, 0);
            _timer = null;
        }
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        lock (_lock)
        {
            _timer?.Dispose();
        }
    }

    public async Task RescheduleAsync(CancellationToken ct = default)
    {
        LogHelper.Info("🔄 ProjectExpiryScheduler reschedule requested...", _log);
        await ScheduleNextCheckAsync(ct);
    }

    private async Task CheckExpiredProjects(CancellationToken ct = default)
    {
        LogHelper.Info($"⏱ Running CheckExpiredProjects at {SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc()}", _log);
        using var scope = _scopeFactory.CreateScope();
        var teamProjectLife = scope.ServiceProvider.GetRequiredService<ITeamProjectLifeCycle>();
        await teamProjectLife.RemoveProjects(ct);
        await ScheduleNextCheckAsync(ct);
    }

    private async Task ScheduleNextCheckAsync(CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var teamProjectLifeCycle = scope.ServiceProvider.GetRequiredService<ITeamProjectLifeCycle>();

        // On récupère la prochaine expiration (Instant? ou null)
        var nextExpiration = await teamProjectLifeCycle.GetNextProjectExpirationDate(ct);

        if (!nextExpiration.HasValue)
        {
            LogHelper.Info("⏸ No future project expirations found. Timer stopped.", _log);
            lock (_lock)
            {
                _timer?.Change(Timeout.Infinite, Timeout.Infinite);
                _timer = null;
            }
            return;
        }

        // On calcule 3 minutes avant la date d'expiration
        var now = NodaTime.SystemClock.Instance.GetCurrentInstant();
        var targetInstant = nextExpiration.Value - Duration.FromMinutes(3);

        var delay = targetInstant > now
            ? targetInstant - now
            : Duration.Zero;

        var delayMs = (long)delay.TotalMilliseconds;

        LogHelper.Info($"▶️ Next project expiry check scheduled for {targetInstant.ToDateTimeUtc()} (in {delayMs / 1000}s)", _log);

        lock (_lock)
        {
            _timer?.Dispose();
            _timer = new Timer(
                async _ =>
                {
                    try
                    {
                        await CheckExpiredProjects(ct);
                    }
                    catch (Exception ex)
                    {
                        _log.LogError(ex, "❌ Error while checking expired projects");
                    }
                },
                null,
                TimeSpan.FromMilliseconds(delayMs),
                Timeout.InfiniteTimeSpan
            );
        }
    }
}
