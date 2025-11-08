using Teams.APP.Layer.Helpers;
using Teams.APP.Layer.Interfaces;
using Teams.INFRA.Layer.Dispatchers;
using Teams.INFRA.Layer.Interfaces;
using NodaTime;
using Teams.CORE.Layer.Entities.TeamAggregate.TeamExtensionMethods;


namespace Teams.APP.Layer.Services.Scheldulers;

/// <summary>
/// Scheduler to manage team lifecycle events such as maturity and expiration.
/// 1. Checks for team maturity and expiration at scheduled intervals.
///     - Maturity is determined by a predefined duration since team creation.
///     - Expiration is based on the team's expiration date.
/// 2. Archives teams that have expired and have no active dependencies.
/// 3. Reschedules the next check based on the nearest upcoming maturity or expiration date.
///    Uses a Timer to trigger checks and ensures thread safety with locking.
///    Logs key actions and errors for monitoring and debugging.
///    Designed to be started and stopped as a hosted service within the application.
///    Interacts with the team repository and caching services to manage team states.
///    Note: Time intervals are shortened for testing purposes; adjust as needed for production.
/// </summary>
public class TeamMaturityScheduler(
    IServiceScopeFactory _scopeFactory,
    IDomainEventDispatcher _dispatcher,
    ILogger<TeamMaturityScheduler> _log
) : IHostedService, IDisposable, ITeamMaturityScheduler
{
    private Timer? _timer;
    private readonly object _lock = new();
    private DateTimeOffset? _nextCheckDate;


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
        lock (_lock) _timer?.Dispose();
    }

    public async Task RescheduleAsync(CancellationToken ct = default)
    {
        LogHelper.Info("🔄 Reschedule requested...", _log);
        await ScheduleNextCheckAsync();
    }
    private async Task CheckTeams(CancellationToken ct = default)
    {
        LogHelper.Info($" ⏱ Running team reach maturity check at {SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc().ToLocalTime()}", _log);

        using var scope = _scopeFactory.CreateScope();
        var redisCacheService = scope.ServiceProvider.GetRequiredService<IRedisCacheService>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var teams = unitOfWork.TeamRepository.GetAll(ct);
        var matureTeams = teams.GetMatureTeams();
        foreach (var team in matureTeams)
        {
            unitOfWork.TeamRepository.Update(team);
            await _dispatcher.DispatchAsync(team.DomainEvents, ct);
            team.ClearDomainEvents();
        }
        await ScheduleNextCheckAsync();
    }

    private async Task ScheduleNextCheckAsync(CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var teams = unitOfWork.TeamRepository.GetAll(ct);
        // Calcul des prochaines dates (maturité + expiration)
        var futureMaturities = teams.GetfutureMaturities();
        var nextEvents = futureMaturities.ToList();
        if (!nextEvents.Any())
        {
            LogHelper.Info("⏸ No upcoming maturities teams found. Timer stopped.", _log);
            lock (_lock) _timer = null;
            return;
        }
        _nextCheckDate = nextEvents.Min();
        var delay = _nextCheckDate.Value - DateTimeOffset.Now;
        if (delay < TimeSpan.Zero)
            delay = TimeSpan.Zero;

        LogHelper.Info(
            $"▶️ Next team maturity check scheduled for {_nextCheckDate:yyyy-MM-dd HH:mm:ss} (in {delay.TotalSeconds}s)",
            _log
        );
        lock (_lock)
        {
            _timer?.Dispose();
            _timer = new Timer(
                async _ =>
                {
                    try
                    {
                        await CheckTeams();
                    }
                    catch (Exception ex)
                    {
                        _log.LogError(ex, "❌ Error while checking team maturity");
                    }
                },
                null,
                delay,
                Timeout.InfiniteTimeSpan
            );
        }
    }
}
