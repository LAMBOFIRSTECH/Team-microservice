using Teams.APP.Layer.DTOs.Output;
using Teams.CORE.Layer.CoreEvents.TeamEvents;
using Teams.APP.Layer.Helpers;
using Microsoft.Extensions.Logging;
using Teams.CORE.Layer.CoreInterfaces;
namespace Teams.APP.Layer.EventHandlers;

// Redis n'a rien à faire ici c'est dans l'infra, 
public class TeamArchiveEventHandler(IRedisCacheService cache, INotificationService notification, ILogger<TeamArchiveEventHandler> _log) : IDomainEventHandler<TeamArchiveEvent>
{
    public async Task Handle(TeamArchiveEvent @event, CancellationToken ct)
    {
        try
        {
            LogHelper.Info($"📦 Archiving team {@event.TeamName} (ID: {@event.TeamId}) in Redis Cache memory for 7 days.", _log);
            var redisTeamDto = new TeamDtoModels.TeamDetailsDto(
                Id: @event.TeamId,
                Name: @event.TeamName,
                TeamManagerId: Guid.Empty, // Default value for archived team
                TeamCreationDate: string.Empty, // Not available in archive event
                TeamExpirationDate: @event.ArchivedAt.ToString(),
                HasAnyProject: false, // Default for archived team
                ProjectNames: null, // No projects for archived team
                State: "Archived"
            );
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
