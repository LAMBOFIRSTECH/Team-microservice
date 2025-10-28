using AutoMapper;
using Teams.API.Layer.DTOs;
using Teams.APP.Layer.Helpers;
using Teams.APP.Layer.Interfaces;
using Teams.INFRA.Layer.Dispatchers;
using Teams.CORE.Layer.CoreServices;
using Teams.INFRA.Layer.Interfaces;
using NodaTime;

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
public class TeamLifeCycleScheduler(
    IServiceScopeFactory _scopeFactory,
    IDomainEventDispatcher _dispatcher,
    IMapper _mapper,
    ILogger<TeamLifeCycleScheduler> _log
) : IHostedService, IDisposable, ITeamLifecycleScheduler
{
    private Timer? _timer;
    private readonly object _lock = new();
    private DateTimeOffset? _nextCheckDate;


    public async Task StartAsync(CancellationToken ct)
    {
        LogHelper.Info("🚀 TeamLifecycleScheduler starting...", _log);
        await ScheduleNextCheckAsync();
    }
    public Task StopAsync(CancellationToken ct)
    {
        LogHelper.Info("🛑 TeamLifecycleScheduler stopping timer...", _log);
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
        LogHelper.Info($" ⏱ Running CheckTeams at {SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc().ToLocalTime()}", _log);

        using var scope = _scopeFactory.CreateScope();
        var redisCacheService = scope.ServiceProvider.GetRequiredService<IRedisCacheService>();
        var teamLifeCycleCoreService = scope.ServiceProvider.GetRequiredService<TeamLifeCycleCoreService>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var teams = unitOfWork.TeamRepository.GetAll(ct);
        var matureTeams = teamLifeCycleCoreService.GetMatureTeams(teams);
        foreach (var team in matureTeams)
        {
            unitOfWork.TeamRepository.Update(team);
            await _dispatcher.DispatchAsync(team.DomainEvents, ct);
            team.ClearDomainEvents();
        }
        var expiredTeams = teamLifeCycleCoreService.GetExpiredTeams(teams);
        teamLifeCycleCoreService.ArchiveTeams(expiredTeams);
        foreach (var team in expiredTeams)
        {
            unitOfWork.TeamRepository.Update(team);
            LogHelper.Info($"📦 Archiving team {team.Name} in Redis Cache memory for 7 days.", _log);
            var redisTeamDto = _mapper.Map<TeamDetailsDto>(team);
            await redisCacheService.StoreArchivedTeamInRedisAsync(redisTeamDto, ct);
            // send notification event (via domain event)  
            await _dispatcher.DispatchAsync(team.DomainEvents, ct); // pertinence qu'à meme
            team.ClearDomainEvents(); //  pertinence de supprimer ??
            LogHelper.Info($"🔔 Notification for archived team {team.Name} sent.", _log);
        }
        await ScheduleNextCheckAsync();
    }

    private async Task ScheduleNextCheckAsync(CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var teamLifeCycleCoreService = scope.ServiceProvider.GetRequiredService<TeamLifeCycleCoreService>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var teams = unitOfWork.TeamRepository.GetAll(ct);
        // Calcul des prochaines dates (maturité + expiration)
        var futureMaturities = teamLifeCycleCoreService.GetfutureMaturities(teams);
        var futureExpirations = teamLifeCycleCoreService.GetfutureExpirations(teams);
        var nextEvents = futureMaturities.Concat(futureExpirations).ToList();
        if (!nextEvents.Any())
        {
            LogHelper.Info("⏸ No upcoming maturities or expirations teams found. Timer stopped.", _log);
            lock (_lock) _timer = null;
            return;
        }
        _nextCheckDate = nextEvents.Min();
        var delay = _nextCheckDate.Value - DateTimeOffset.Now;
        if (delay < TimeSpan.Zero)
            delay = TimeSpan.Zero;

        LogHelper.Info(
            $"▶️ Next team lifecycle check scheduled for {_nextCheckDate:yyyy-MM-dd HH:mm:ss} (in {delay.TotalSeconds}s)",
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
                        _log.LogError(ex, "❌ Error while checking team lifecycle");
                    }
                },
                null,
                delay,
                Timeout.InfiniteTimeSpan
            );
        }
    }
}
