using Teams.APP.Layer.DTOs.Output;
using Teams.APP.Layer.Interfaces;
using Teams.CORE.Layer.CoreEvents.TeamEvents;
using Teams.APP.Layer.Helpers;
using Microsoft.Extensions.Logging;
using Teams.CORE.Layer.CoreInterfaces;
namespace Teams.APP.Layer.EventHandlers;

public class TeamCreatedEventHandler : IDomainEventHandler<TeamCreatedEvent>
{
    private readonly IRedisCacheService _cache;
    private readonly INotificationService _notification;
    private readonly ILogger<TeamCreatedEventHandler> _log;

    public TeamCreatedEventHandler(
        IRedisCacheService cache,
        INotificationService notification,
        ILogger<TeamCreatedEventHandler> log
    )
    {
        _cache = cache;
        _notification = notification;
        _log = log;
    }

    public async Task Handle(TeamCreatedEvent @event, CancellationToken ct)
    {
        try
        {
            LogHelper.Info($"📦 Creating team {@event.TeamName} (ID: {@event.TeamId}) in Redis Cache memory.", _log);
            var redisTeamDto = new TeamDtoModels.TeamDetailsDto(
                Id: @event.TeamId,
                Name: @event.TeamName,
                TeamManagerId: Guid.Empty,
                TeamCreationDate: @event.CreatedAt.ToString(),
                TeamExpirationDate: string.Empty,
                HasAnyProject: false,
                ProjectNames: null,
                State: "Active"
            );

            await _cache.StoreArchivedTeamInRedisAsync(redisTeamDto, ct);
            await _notification.NotifyTeamArchived(@event.TeamId, ct);
            LogHelper.Info($"🔔 Notification for archived team {@event.TeamName} sent successfully.", _log);
        }
        catch (Exception ex)
        {
            LogHelper.Error($"❌ Failed to handle TeamArchiveEvent for {@event.TeamName}: {ex.Message}", _log);
            throw;
        }
    }
}
