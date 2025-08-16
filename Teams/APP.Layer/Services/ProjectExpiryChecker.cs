using Teams.APP.Layer.Helpers;
using Teams.CORE.Layer.Interfaces;

namespace Teams.APP.Layer.Services;

public class ProjectExpiryChecker : IHostedService, IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ProjectExpiryChecker> _log;
    private Timer? _timer;

    public ProjectExpiryChecker(
        IServiceScopeFactory scopeFactory,
        ILogger<ProjectExpiryChecker> log
    )
    {
        _scopeFactory = scopeFactory;
        _log = log;
    }

    public void EnsureTimerStarted()
    {
        if (_timer != null)
            return; // déjà démarré

        LogHelper.Info("▶️   ProjectExpiryChecker timer started after first team creation", _log);
        _timer = new Timer(CheckExpiredProjects, null, TimeSpan.Zero, TimeSpan.FromMinutes(2));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var teamRepository = scope.ServiceProvider.GetRequiredService<ITeamRepository>();
        var teams = await teamRepository.GetAllTeamsAsync();
        if (teams.Any())
            EnsureTimerStarted();

        LogHelper.Info("💤 ProjectExpiryChecker not started: no teams found at startup.", _log);
    }

    private async void CheckExpiredProjects(object? state)
    {
        LogHelper.Info(
            $"⏱ ProjectExpiryChecker running CheckExpiredProjects at {DateTime.Now}",
            _log
        );

        using var scope = _scopeFactory.CreateScope();
        var teamRepository = scope.ServiceProvider.GetRequiredService<ITeamRepository>();

        var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
        var teams = await teamRepository.GetAllTeamsAsync();
        _log.LogInformation($"Numbers of teams {teams.Count}...");
        var expiredTeams = teams
            .Where(t => t.ProjectEndDate.HasValue && t.ProjectEndDate.Value <= now)
            .ToList();
        var ateam = teams.Where(t => t.Name.Equals("Pentester")).FirstOrDefault();
        var titi = ateam != null ? $"{ateam.ProjectEndDate} + toto" : "ateam is null + toto";
        _log.LogInformation($"Debut de la boucle {titi}");

        foreach (var team in expiredTeams)
        {
            _log.LogInformation("Disassociating project from team {TeamName}", team.Name);
            team.RemoveProjectFromTeamWhenExpired(true);
            team.RecalculateState();
            await teamRepository.UpdateTeamAsync(team);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        LogHelper.Info("🛑 ProjectExpiryChecker stopping timer...", _log);
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
