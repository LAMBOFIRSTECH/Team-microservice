using AutoMapper;
using Teams.API.Layer.DTOs;
using Teams.APP.Layer.Helpers;
using Teams.APP.Layer.Interfaces;
using Teams.CORE.Layer.Interfaces;
using Teams.INFRA.Layer.Dispatchers;
using Teams.CORE.Layer.CoreServices;

namespace Teams.APP.Layer.Services;

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
public class TeamLifecycleScheduler(
    IServiceScopeFactory _scopeFactory,
    IDomainEventDispatcher dispatcher,
    IMapper mapper,
    TeamLifecycleDomainService teamLifecycleDomain,
    ILogger<TeamLifecycleScheduler> _log
) : IHostedService, IDisposable, ITeamLifecycleScheduler
{
    private Timer? _timer;
    private readonly object _lock = new();
    private DateTime? _nextCheckDate;


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
        lock (_lock)
        {
            _timer?.Dispose();
        }
    }

    public async Task RescheduleAsync(CancellationToken ct = default)
    {
        LogHelper.Info("🔄 Reschedule requested...", _log);
        await ScheduleNextCheckAsync();
    }

    private async Task CheckTeams(CancellationToken ct = default)
    {
        LogHelper.Info($" ⏱ Running CheckTeams at {DateTime.Now}", _log);

        using var scope = _scopeFactory.CreateScope();
        var redisCacheService = scope.ServiceProvider.GetRequiredService<IRedisCacheService>();
        var teamRepository = scope.ServiceProvider.GetRequiredService<ITeamRepository>();
        var teams = await teamRepository.GetAllTeamsAsync(ct, asNoTracking: true);

        var matureTeams = teamLifecycleDomain.GetMatureTeams(teams);
        foreach (var team in matureTeams)
        {
            // if (!teamLifecycleDomain.MatureTeams(teams))
            //     LogHelper.Info($"✅ Team {team.Name} is not yet mature", _log);
            // else
            //     LogHelper.Info($"✅ Team {team.Name} reached maturity", _log);
            await teamRepository.UpdateTeamAsync(team, ct);
            await dispatcher.DispatchAsync(team.DomainEvents, ct);
            team.ClearDomainEvents();
        }
        var expiredTeams = teamLifecycleDomain.GetExpiredTeams(teams);
        teamLifecycleDomain.ArchiveTeams(expiredTeams);
        foreach (var team in expiredTeams)
        {
            /**
               Comportement bizarre : Quand il y'a une dépendance la date d'expiration n'est pas mise à jour
               Détail :
                        Donc on archive l'équipe seulement si elle n'a pas de dépendance
                        Cependant quand le projet arrive à expiration il reprend la date de création + 180 jours
                        Exemple : "teamCreationDate"  :"28-09-2025 15:27:40",
                                  "teamExpirationDate": "30-10-2025 10:00:00"  -> C'est la date du projet
                Conclusion :
                            Quand cette date est dépassée et bien teamExpirationDate redevient "28-09-2026 15:27:40" + la validationPeriode
                            Ce qui est illogique car l'équipe a déjà été créée depuis longtemps
                Trouver le moyen de fixer une date d'expiration exactement à la date de fin du projet de façon à ce que
                Le scheduler puisse archiver l'équipe au meme moment qu'il supprime le projet expiré de l'équipe
            **/
            await teamRepository.UpdateTeamAsync(team, ct);
            LogHelper.Info($"📦 Archiving team {team.Name} in Redis Cache memory for 7 days.", _log);
            var redisTeamDto = mapper.Map<TeamDetailsDto>(team);
            await redisCacheService.StoreArchivedTeamInRedisAsync(redisTeamDto, ct);
            // send notification event (via domain event)  
            await dispatcher.DispatchAsync(team.DomainEvents, ct); // pertinence qu'à meme
            team.ClearDomainEvents(); //  pertinence de supprimer ??
            LogHelper.Info($"🔔 Notification for archived team {team.Name} sent.", _log);
        }
        await ScheduleNextCheckAsync();
    }

    private async Task ScheduleNextCheckAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var teamRepository = scope.ServiceProvider.GetRequiredService<ITeamRepository>();
        var teams = await teamRepository.GetAllTeamsAsync();

        // Calcul des prochaines dates (maturité + expiration)
        var futureMaturities = teamLifecycleDomain.GetfutureMaturities(teams);
        var futureExpirations = teamLifecycleDomain.GetfutureExpirations(teams);
        var nextEvents = futureMaturities.Concat(futureExpirations).ToList();
        if (!nextEvents.Any())
        {
            LogHelper.Info("⏸ No upcoming maturities or expirations. Timer stopped.", _log);
            lock (_lock)
            {
                _timer = null;
            }
            return;
        }

        _nextCheckDate = nextEvents.Min();
        var delay = _nextCheckDate.Value - DateTime.Now;
        if (delay < TimeSpan.Zero)
            delay = TimeSpan.Zero;

        LogHelper.Info(
            $"▶️ Next lifecycle check scheduled for {_nextCheckDate} (in {delay.TotalSeconds}s)",
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
