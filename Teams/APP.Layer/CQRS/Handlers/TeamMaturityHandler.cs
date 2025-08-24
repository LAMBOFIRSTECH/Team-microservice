// using MediatR;
// using Teams.APP.Layer.EventNotification;
// using Teams.APP.Layer.Interfaces;
// using Teams.CORE.Layer.CoreEvents;

// namespace Teams.APP.Layer.CQRS.Handlers;

// public class TeamMaturityHandler(ITeamMaturityScheduler _scheduler)
//     : INotificationHandler<DomainEventNotification<TeamMaturityEvent>>
// {
//     public async Task Handle(
//         DomainEventNotification<TeamMaturityEvent> notification,
//         CancellationToken ct
//     )
//     {
//         // Une date a changé → on recalcule la prochaine échéance et on reprogramme le timer
//         await _scheduler.RescheduleMaturityTeamAsync(ct);
//     }
// }
