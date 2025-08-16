// using Hangfire;
// using MediatR;
// using Teams.APP.Layer.Helpers;
// using Teams.APP.Layer.Interfaces;
// using Teams.CORE.Layer.CoreEvents;
// using Teams.CORE.Layer.ValueObjects;
// using Teams.INFRA.Layer.ExternalServicesDtos;

// namespace Teams.APP.Layer.CQRS.Handlers;

// public class ProjectAssociatedHandler(
//     ILogger<ProjectAssociatedHandler> log,
//     IServiceScopeFactory scopeFactory
// ) : INotificationHandler<ProjectAssociatedNotification>
// {
//     public async Task Handle(
//         ProjectAssociatedNotification notification,
//         CancellationToken cancellationToken
//     )
//     {
//         LogHelper.Info(
//             $"[ProjectAssociatedHandler] Handling association for {notification.ProjectName}",
//             log
//         );
//         var result = notification.EventMessage;

//         using var scope = scopeFactory.CreateScope();
//         var backgroundJob = scope.ServiceProvider.GetRequiredService<IBackgroundJobService>();

//         if (notification.CreatedAt > DateTime.UtcNow)
//         {
//             LogHelper.Info($"Scheduling disassociation at {notification.CreatedAt:u}", log);

//             BackgroundJob.Schedule(
//                 () =>
//                     backgroundJob.DisAffectedProjectToTeam(
//                         notification.ProjectName,
//                         notification.TeamManagerId
//                     ),
//                 notification.CreatedAt - DateTime.UtcNow
//             );
//         }
//         else
//         {
//             LogHelper.Info("Project end date already passed, running immediately.", log);
//             BackgroundJob.Enqueue(() =>
//                 backgroundJob.DisAffectedProjectToTeam(
//                     notification.,
//                     notification.TeamManagerId
//                 )
//             );
//         }
//     }
// }
