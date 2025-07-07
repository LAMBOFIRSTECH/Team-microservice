// using System;
// using Teams.APP.Layer.CQRS.Events;
// using Teams.APP.Layer.Services;

// namespace Teams.APP.Layer.CQRS.Handlers;

// public class ManageTeamEventHandler
//     : IEventHandler<EmployeeCreatedEvent>,
//         IEventHandler<ProjectAssociatedEvent>
// {
//     private readonly IBackgroundJobService _backgroundJobService;

//     public ManageTeamEventHandler(IBackgroundJobService backgroundJobService)
//     {
//         _backgroundJobService = backgroundJobService;
//     }

//     public async Task HandleAsync(EmployeeCreatedEvent @event)
//     {
//         // Lorsque l'événement EmployeeCreatedEvent est capturé, on planifie l'ajout du membre
//         await _backgroundJobService.ScheduleAddTeamMemberAsync(@event.EmployeeId);
//     }

//     public async Task HandleAsync(ProjectAssociatedEvent @event)
//     {
//         // Lorsque l'événement ProjectAssociatedEvent est capturé, on planifie l'association du projet
//         await _backgroundJobService.ScheduleProjectAssociationAsync(@event.EmployeeId);
//     }
// }
