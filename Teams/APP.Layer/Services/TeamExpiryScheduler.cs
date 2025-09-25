using Teams.APP.Layer.Helpers;
using Teams.APP.Layer.Interfaces;
using Teams.CORE.Layer.BusinessExceptions;
using Teams.CORE.Layer.Interfaces;

namespace Teams.APP.Layer.Services;

public class TeamExpiryScheduler(
    IServiceScopeFactory _scopeFactory,
    ILogger<TeamExpiryScheduler> _log
) : IHostedService, IDisposable, ITeamExpiryScheduler
{
    private Timer? _timer;
    private DateTime? _nextTeamExpiration;

    public async Task StartAsync(CancellationToken ct)
    {
        LogHelper.Info("🚀 TeamExpiryScheduler starting...", _log);
        await ScheduleNextCheckAsync();
    }

    public Task StopAsync(CancellationToken ct)
    {
        LogHelper.Info("🛑 TeamExpiryScheduler stopping timer...", _log);
        _timer?.Change(Timeout.Infinite, 0);
        _timer = null;
        return Task.CompletedTask;
    }

    public void Dispose() => _timer?.Dispose();

    public async Task RescheduleAsync(CancellationToken ct)
    {
        LogHelper.Info("🔄 Reschedule requested...", _log);
        await ScheduleNextCheckAsync();
    }

    private async Task CheckExpiredTeams(CancellationToken ct = default)
    {
        LogHelper.Info($"⏱ Running CheckExpiredTeams at {DateTime.Now}", _log);

        using var scope = _scopeFactory.CreateScope();
        var teamRepository = scope.ServiceProvider.GetRequiredService<ITeamRepository>();
        var _redisCacheService = scope.ServiceProvider.GetRequiredService<IRedisCacheService>();
        var now = DateTime.Now;

        var teams = await teamRepository.GetAllTeamsAsync(ct, asNoTracking: true);
        var expiredTeams = teams.Where(t => t.IsTeamExpired()).ToList();

        foreach (var team in teams)
        {
            if (team.HasAnyDependencies())
            {
                LogHelper.Warning(
                    $"⚠️ Team {team.Name} cannot be archived because it has active projects.",
                    _log
                );
                throw new DomainException(
                    $"The team {team.Name} cannot be archived because it has active projects."
                );
            }
            team.ArchiveTeam();
            await teamRepository.UpdateTeamAsync(team, ct);
            LogHelper.Info($"Archiving team {team.Name}...", _log);
            await _redisCacheService.StoreArchivedTeamInRedisAsync(team, ct);
        }
        await ScheduleNextCheckAsync();
    }

    private async Task ScheduleNextCheckAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var teamRepository = scope.ServiceProvider.GetRequiredService<ITeamRepository>();

        var now = DateTime.Now;
        var teams = await teamRepository.GetAllTeamsAsync();
        var futureExpirations = teams
            .Where(t => t.ExpirationDate > now)
            .Select(t => t.ExpirationDate)
            .ToList();

        if (!futureExpirations.Any())
        {
            LogHelper.Info("⏸ No upcoming team expirations. Timer stopped.", _log);
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            _timer = null;
            return;
        }

        _nextTeamExpiration = futureExpirations.Min();
        var delay = _nextTeamExpiration.Value - now;
        if (delay < TimeSpan.Zero)
            delay = TimeSpan.Zero;

        LogHelper.Info(
            $"▶️  Next check scheduled for {_nextTeamExpiration} (in {delay.TotalSeconds}s)",
            _log
        );

        _timer?.Dispose();
        _timer = new Timer(
            async _ =>
            {
                try
                {
                    await CheckExpiredTeams();
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "❌ Error while checking expired teams");
                }
            },
            null,
            delay,
            Timeout.InfiniteTimeSpan
        );
    }
}
