// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Logging;
// using System.Threading;
// using System.Threading.Tasks;
// using Teams.APP.Layer.Interfaces;

// namespace Teams.APP.Layer.Services.Schedulers;

// /// <summary>
// /// This orchestrator manages multiple timers for different scheduling tasks within the application.
// /// It starts, stops, and disposes of individual schedulers such as ProjectExpiryScheduler and TeamLifeCycleScheduler.
// /// It also provides a method to reschedule all timers, allowing for dynamic adjustment of scheduling based on application needs.
// /// </summary>
// public class TimerOrchestrator(IServiceScopeFactory _scopeFactory, ILogger<TimerOrchestrator> _log) 
// {

//     private IProjectExpirySchedule? _projectExpiryScheduler;
//     private ITeamLifecycleScheduler? _teamLifecycleScheduler;
//      public async Task StartAsync(CancellationToken ct = default)
//     {
//         _log.LogInformation("🚀 TimerOrchestrator starting...");

//         using var scope = _scopeFactory.CreateScope();
//         _projectExpiryScheduler = scope.ServiceProvider.GetRequiredService<IProjectExpirySchedule>();
//         _teamLifecycleScheduler = scope.ServiceProvider.GetRequiredService<ITeamLifecycleScheduler>();

//         await ConditionalStartAsync(ct);
//     }

//     public async Task RescheduleAllAsync(CancellationToken ct = default)
//     {
//         await ConditionalStartAsync(ct);
//     }

//     private async Task ConditionalStartAsync(CancellationToken ct)
//     {
//         if (_projectExpiryScheduler == null || _teamLifecycleScheduler == null) return;

//         var nextProjectExpiration = await _projectExpiryScheduler.GetNextExpirationInstant();
//         var nextTeamMaturity = await _teamLifecycleScheduler.GetNextMaturityInstant();

//         if (nextProjectExpiration.HasValue && nextTeamMaturity.HasValue)
//         {
//             if (nextProjectExpiration.Value < nextTeamMaturity.Value)
//             {
//                 _log.LogInformation("🔄 ProjectExpiryScheduler triggered first");
//                 await _projectExpiryScheduler.RescheduleAsync(ct);
//                 await _teamLifecycleScheduler.RescheduleAsync(ct);
//             }
//             else
//             {
//                 _log.LogInformation("🔄 TeamLifecycleScheduler triggered first");
//                 await _teamLifecycleScheduler.RescheduleAsync(ct);
//                 await _projectExpiryScheduler.RescheduleAsync(ct);
//             }
//         }
//         else if (nextProjectExpiration.HasValue)
//         {
//             await _projectExpiryScheduler.RescheduleAsync(ct);
//         }
//         else if (nextTeamMaturity.HasValue)
//         {
//             await _teamLifecycleScheduler.RescheduleAsync(ct);
//         }
//         else
//         {
//             _log.LogInformation("⏸ No upcoming project expirations or team maturities found. Nothing to schedule.");
//         }
//     }
// }



