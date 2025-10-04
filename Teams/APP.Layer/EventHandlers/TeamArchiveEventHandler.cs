// using AutoMapper;
// using Teams.API.Layer.DTOs;
// using Teams.APP.Layer.Interfaces;
// using Teams.CORE.Layer.CoreEvents;
// using Teams.CORE.Layer.CoreInterfaces;
// namespace Teams.APP.Layer.EventHandlers;
// // C'est ici qu'on va gérer les caches et les notifications externes
// public class TeamArchiveEventHandler(IRedisCacheService cache, INotificationService notification, IMapper mapper) : IDomainEvent<TeamArchiveEvent> // à la place mettre : IDomainEventHandler<TeamArchiveEvent>
// {
//     public async Task Handle(TeamArchiveEvent @event, CancellationToken ct)
//     {
//         // Option A: le event contient assez d'info pour créer le DTO -> on évite une nouvelle DB hit
//         var dto = new TeamDetailsDto { Id = @event.TeamId, Name = @event.TeamName, TeamExpirationDate = @event.ArchivedAt.ToString() };
//         await cache.StoreArchivedTeamInRedisAsync(dto, ct);
//         await notification.NotifyTeamArchived(@event.TeamId, ct);
//     }
// }
