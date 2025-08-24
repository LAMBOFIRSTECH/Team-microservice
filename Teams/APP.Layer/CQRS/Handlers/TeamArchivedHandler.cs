// using MediatR;
// using Teams.APP.Layer.EventNotification;
// using Teams.APP.Layer.Interfaces;
// using Teams.CORE.Layer.CoreEvents;

// public class TeamArchivedHandler(IScheduler _scheduler, ILogger<TeamArchivedHandler> _log)
//     : INotificationHandler<DomainEventNotification<TeamArchiveEvent>>
// {
//     public async Task Handle(
//         DomainEventNotification<TeamArchiveEvent> notification,
//         CancellationToken ct
//     )
//     {
//         _log.LogInformation(
//             "📦 TeamArchiveEvent received for Team {TeamId} (archived), rescheduling...",
//             notification.DomainEvent.TeamId
//         );
//         await _scheduler.RescheduleAsync(ct);
//     }
// }
