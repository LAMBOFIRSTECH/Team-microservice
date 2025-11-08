using Teams.API.Layer.DTOs;
using Teams.APP.Layer.Interfaces;
using Teams.CORE.Layer.CoreEvents.TeamEvents;
using Teams.APP.Layer.Helpers;
using Teams.INFRA.Layer.Interfaces;
namespace Teams.APP.Layer.EventHandlers;

public class TeamArchiveEventHandler(IRedisCacheService cache, INotificationService notification, ILogger<TeamArchiveEventHandler> _log) : IDomainEventHandler<TeamArchiveEvent>
{
    public async Task Handle(TeamArchiveEvent @event, CancellationToken ct)
    {
        try
        {
            LogHelper.Info($"📦 Archiving team {@event.TeamName} (ID: {@event.TeamId}) in Redis Cache memory for 7 days.", _log);
            var redisTeamDto = new TeamDetailsDto { Id = @event.TeamId, Name = @event.TeamName, TeamExpirationDate = @event.ArchivedAt.ToString() };
            await cache.StoreArchivedTeamInRedisAsync(redisTeamDto, ct);
            await notification.NotifyTeamArchived(@event.TeamId, ct);
            LogHelper.Info($"🔔 Notification for archived team {@event.TeamName} sent successfully.", _log);
        }
        catch (Exception ex)
        {
            LogHelper.Error($"❌ Failed to handle TeamArchiveEvent for {@event.TeamName}: {ex.Message}", _log);
            throw; // on relance pour que le dispatcher sache que c’est échoué
        }
    }

}
