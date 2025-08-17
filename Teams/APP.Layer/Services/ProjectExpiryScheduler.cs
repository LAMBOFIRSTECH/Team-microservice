using Teams.APP.Layer.Helpers;
using Teams.APP.Layer.Interfaces;
using Teams.CORE.Layer.Interfaces;

namespace Teams.APP.Layer.Services;

public class ProjectExpiryScheduler(
    IServiceScopeFactory _scopeFactory,
    ILogger<ProjectExpiryScheduler> _log
) : IHostedService, IDisposable, IProjectExpiryScheduler
{
    private Timer? _timer;
    private DateTime? _nextProjectDateExpiration;

    // =============================
    // 🟢 Start
    // =============================
    public async Task StartAsync(CancellationToken ct)
    {
        LogHelper.Info("🚀 ProjectExpiryChecker starting...", _log);
        await ScheduleNextCheckAsync();
    }

    // =============================
    // 🛑 Stop
    // =============================
    public Task StopAsync(CancellationToken ct)
    {
        LogHelper.Info("🛑 ProjectExpiryChecker stopping timer...", _log);
        _timer?.Change(Timeout.Infinite, 0);
        _timer = null;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    // =============================
    // 🔄 Replanification manuelle
    // =============================
    public async Task RescheduleAsync(CancellationToken ct)
    {
        await ScheduleNextCheckAsync();
    }

    // =============================
    // ⚡ Main processing logic
    // =============================
    private async Task CheckExpiredProjects()
    {
        LogHelper.Info($"⏱ Running CheckExpiredProjects at {DateTime.Now}", _log);

        using var scope = _scopeFactory.CreateScope();
        var teamRepository = scope.ServiceProvider.GetRequiredService<ITeamRepository>();
        var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);

        var teams = await teamRepository.GetAllTeamsAsync();
        var expiredTeams = teams
            .Where(t => t.ProjectEndDate.HasValue && t.ProjectEndDate.Value <= now)
            .ToList();

        foreach (var team in expiredTeams)
        {
            _log.LogInformation(
                "Team {Name} | EndDate={EndDate} | Now={Now} | Expired={Expired} | State={State}",
                team.Name,
                team.ProjectEndDate?.ToString() ?? "null",
                now,
                team.ProjectEndDate.HasValue && team.ProjectEndDate.Value <= now,
                team.State
            );

            team.RemoveProjectFromTeamWhenExpired(true);
            team.RecalculateState();
            await teamRepository.UpdateTeamAsync(team);

            LogHelper.Info($"✅ Project has been dissociated correctly from team {team.Name}", _log);
        }
        await ScheduleNextCheckAsync();
    }

    // =============================
    // 📅 Schedule next execution
    // =============================
    private async Task ScheduleNextCheckAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var teamRepository = scope.ServiceProvider.GetRequiredService<ITeamRepository>();

        var now = DateTime.Now;
        var teams = await teamRepository.GetAllTeamsAsync();

        _nextProjectDateExpiration = teams
            .Where(t => t.ProjectEndDate.HasValue && t.ProjectEndDate.Value > now)
            .Min(t => t.ProjectEndDate);

        if (_nextProjectDateExpiration == null)
        {
            LogHelper.Info("⏸ No future deadlines found. Timer stopped.", _log);
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            _timer = null;
            return;
        }

        var delay = _nextProjectDateExpiration.Value - now;
        if (delay < TimeSpan.Zero)
            delay = TimeSpan.Zero;

        LogHelper.Info($"▶️  Next check scheduled for {_nextProjectDateExpiration}", _log);

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
