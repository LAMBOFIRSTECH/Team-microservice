using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodaTime;
using Teams.APP.Layer.Helpers;

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
        StopTimer();
        return Task.CompletedTask;
    }
    private void StopTimer()
    {
        lock (_lock)
        {
            _timer?.Change(Timeout.Infinite, 0);
            _timer?.Dispose();
            _timer = null;
        }
    }

    public void Dispose() => StopTimer();

    public async Task RescheduleAsync(CancellationToken ct = default)
    {
        LogHelper.Info("🔄 ProjectExpiryScheduler reschedule requested...", _log);
        await ScheduleNextCheckAsync(ct);
    }

    private async Task CheckExpiredProjects(CancellationToken ct = default)
    {
        LogHelper.Info($"⏱ Running CheckExpiredProjects at {SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc().ToLocalTime()}", _log);
        using var scope = _scopeFactory.CreateScope();
        var teamProjectLife = scope.ServiceProvider.GetRequiredService<ITeamProjectLifeCycle>();
        await teamProjectLife.RemoveProjects(ct);
        await ScheduleNextCheckAsync(ct);
    }

    private async Task ScheduleNextCheckAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var teamProjectLifeCycle = scope.ServiceProvider.GetRequiredService<ITeamProjectLifeCycle>();
        var nextExpiration = await teamProjectLifeCycle.GetNextProjectExpirationDate(ct);

        if (!nextExpiration.HasValue)
        {
            LogHelper.Info("⏸ No future project expirations found. Timer stopped.", _log);
            StopTimer();
            return;
        }

        // Convertir Instant en DateTime local pour un calcul correct du délai
        var targetLocal = nextExpiration.Value;
        var delay = targetLocal - DateTime.UtcNow;
        Console.WriteLine($"delay : {delay}");

        if (delay <= TimeSpan.Zero)
        {
            LogHelper.Info("⚠️ Expiration just reached. Will recheck in 1 minute.", _log);
            ResetTimer(TimeSpan.FromMinutes(1), async _ => await ScheduleNextCheckAsync(ct));
            return;
        }

        LogHelper.Info(
            $"▶️ Next project expiry check scheduled for {targetLocal:yyyy-MM-dd HH:mm:ss} (in {delay.TotalMinutes:F1} min)",
            _log
        );

        ResetTimer(delay, async _ => await CheckExpiredProjects(ct));
    }
    private void ResetTimer(TimeSpan dueTime, Func<object?, Task> callback)
    {
        lock (_lock)
        {
            _timer?.Dispose();
            _timer = new Timer(_ =>
            {
                Task.Run(async () =>
                {
                    try
                    {
                        if (callback != null)
                            await callback(_timer);
                    }
                    catch (Exception ex)
                    {
                        _log.LogError(ex, "❌ Timer callback execution failed");
                    }
                });
            }, null, dueTime, Timeout.InfiniteTimeSpan);
        }
    }

}



