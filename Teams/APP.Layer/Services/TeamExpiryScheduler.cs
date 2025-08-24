using Teams.APP.Layer.Helpers;
using Teams.APP.Layer.Interfaces;
using Teams.CORE.Layer.Entities;
using Teams.CORE.Layer.Interfaces;

public class TeamExpiryScheduler(
    IServiceScopeFactory _scopeFactory,
    ILogger<TeamExpiryScheduler> _log
) : IHostedService, IDisposable, ITeamExpiryScheduler
{
    private readonly CancellationTokenSource _cts = new();
    private Task? _backgroundTask;
    private Team? _nextTeamToCheck;

    public Task StartAsync(CancellationToken ct)
    {
        LogHelper.Info("🚀 TeamExpiryScheduler starting...", _log);
        _backgroundTask = RunSchedulerAsync(_cts.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken ct)
    {
        _log.LogInformation("🛑 TeamExpiryScheduler stopping...");
        _cts.Cancel();
        if (_backgroundTask != null)
        {
            try
            {
                await _backgroundTask;
            }
            catch (TaskCanceledException) { }
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    public async Task RescheduleAsync(Team team, CancellationToken ct = default)
    {
        _log.LogInformation("🔄 Reschedule requested for team {TeamName}...", team.Name);
        _cts.Cancel();
        _nextTeamToCheck = team;
        await Task.Delay(50, ct); // léger délai pour in-memory repo
        await StartAsync(ct);
    }

    private async Task RunSchedulerAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                // Récupère le délai jusqu’à la prochaine expiration
                var delay = await GetNextDelayAsync(ct);
                if (delay == Timeout.InfiniteTimeSpan)
                {
                    _log.LogInformation(
                        "⏸ No upcoming expirations, scheduler paused until next reschedule."
                    );
                    return;
                }

                var nextDate = DateTime.Now + delay;
                _log.LogInformation(
                    $"▶️ Next expiration scheduled at {nextDate} (in {delay.TotalSeconds}s)"
                );

                // Attend exactement jusqu’au moment d’expiration
                await Task.Delay(delay, ct);

                // Vérifie et archive les équipes expirées
                _log.LogInformation("⏱ Expiration reached, checking teams for inactivity...");
                await CheckIfTeamInactivity(ct);
            }
            catch (TaskCanceledException)
            {
                _log.LogInformation("⏹ Scheduler cancelled, restarting if requested...");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "❌ Error in scheduler loop, exiting.");
                return;
            }
        }
    }

    private async Task<TimeSpan> GetNextDelayAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ITeamRepository>();

        var now = DateTime.Now;
        var teams = await repo.GetAllTeamsAsync();

        // Seules les équipes encore actives
        var futureExpirations = teams
            .Where(t => t.ExpirationDate > now)
            .Select(t => t.ExpirationDate)
            .ToList();

        if (!futureExpirations.Any())
            return Timeout.InfiniteTimeSpan;

        var next = futureExpirations.Min();
        var delay = next - now;
        _log.LogInformation($"Next team expiration at {delay:N0} seconds from now.");

        return delay < TimeSpan.Zero ? TimeSpan.Zero : delay;
    }

    private async Task CheckIfTeamInactivity(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var teamRepository = scope.ServiceProvider.GetRequiredService<ITeamRepository>();
        var now = DateTime.Now;
        var teams = await teamRepository.GetAllTeamsAsync();

        foreach (var team in teams)
        {
            if (team.ExpirationDate <= now)
            {
                try
                {
                    _log.LogInformation($"Archiving team {team.Name}...");
                    team.EnsureTeamIsWithinValidPeriod();
                    await teamRepository.UpdateTeamAsync(team);
                    _log.LogInformation($"✅ Team {team.Name} archived successfully.");
                }
                catch (Exception ex)
                {
                    _log.LogWarning($"⚠️ Team {team.Name}: {ex.Message}");
                }
            }
        }
    }
}
