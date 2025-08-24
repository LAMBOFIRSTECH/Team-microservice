using Teams.APP.Layer.Helpers;
using Teams.APP.Layer.Interfaces;
using Teams.CORE.Layer.Interfaces;

namespace Teams.APP.Layer.Services;

public class TeamMaturityScheduler(
    IServiceScopeFactory _scopeFactory,
    ILogger<TeamMaturityScheduler> _log
) : IHostedService, IDisposable, ITeamMaturityScheduler
{
    private Timer? _timer;
    private readonly object _lock = new();
    private DateTime? _nextMaturityDate;

    public async Task StartAsync(CancellationToken ct)
    {
        LogHelper.Info("🚀 TeamMaturityScheduler starting...", _log);
        await ScheduleNextCheckAsync();
    }

    public Task StopAsync(CancellationToken ct)
    {
        LogHelper.Info("🛑 TeamMaturityScheduler stopping timer...", _log);
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

    public async Task RescheduleMaturityTeamAsync(CancellationToken ct = default)
    {
        await ScheduleNextCheckAsync();
    }

    private async Task CheckMatureTeams()
    {
        LogHelper.Info($"⏱ Running CheckMatureTeams at {DateTime.Now}", _log);

        using var scope = _scopeFactory.CreateScope();
        var teamRepository = scope.ServiceProvider.GetRequiredService<ITeamRepository>();
        var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);

        var teams = await teamRepository.GetAllTeamsAsync();

        // Sélectionne uniquement les équipes ayant atteint leur maturité
        var matureTeams = teams.Where(t => (now - t.TeamCreationDate).TotalSeconds >= 30).ToList();
        // Pour production : TotalDays >= 180

        foreach (var team in matureTeams)
        {
            LogHelper.Info($"✅ Team {team.Name} reached maturity", _log);
            team.Maturity();
            await teamRepository.UpdateTeamAsync(team);
        }

        // Replanifie uniquement si des équipes ont encore une maturité future
        await ScheduleNextCheckAsync();
    }

    private async Task ScheduleNextCheckAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var teamRepository = scope.ServiceProvider.GetRequiredService<ITeamRepository>();

        var now = DateTime.Now;
        var teams = await teamRepository.GetAllTeamsAsync();

        // Liste des prochaines maturités encore dans le futur
        var futureMaturities = teams
            .Select(t => t.TeamCreationDate.AddSeconds(30)) // en prod : AddDays(180)
            .Where(d => d > now)
            .ToList();

        if (!futureMaturities.Any())
        {
            LogHelper.Info("⏸ No future maturity dates found. Timer stopped.", _log);
            lock (_lock)
            {
                _timer = null;
            }
            return;
        }

        _nextMaturityDate = futureMaturities.Min();

        var delay = _nextMaturityDate.Value - now;
        if (delay < TimeSpan.Zero)
            delay = TimeSpan.Zero;

        LogHelper.Info($"▶️ Next maturity check scheduled for {_nextMaturityDate}", _log);

        lock (_lock)
        {
            _timer?.Dispose();
            _timer = new Timer(
                async _ =>
                {
                    try
                    {
                        await CheckMatureTeams();
                    }
                    catch (Exception ex)
                    {
                        _log.LogError(ex, "❌ Error while checking mature teams");
                    }
                },
                null,
                delay,
                Timeout.InfiniteTimeSpan // One-shot
            );
        }
    }
}
