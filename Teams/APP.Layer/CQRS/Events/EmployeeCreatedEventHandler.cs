using Teams.API.Layer.DTOs;
using Teams.APP.Layer.Interfaces;
// Add the correct using directive if IEventHandler exists in another namespace, for example:
// using Teams.APP.Layer.CQRS.Interfaces;

// This class is responsible for handling team-related events, such as adding a new team member.
// C'est là où l'on gère les événements liés aux équipes, comme l'ajout d'un nouveau membre à une équipe.

// If IEventHandler does not exist, define it here or in a separate file:
namespace Teams.APP.Layer.CQRS.Events;

// public class EmployeeCreatedEventHandler(IEmployeeService employeeService)
//     : IEventHandler<EmployeeCreatedEvent>
// {
//     public async Task HandleAsync(EmployeeCreatedEvent @event)
//     {
//         await employeeService.AddTeamMemberAsync(@event.EmployeeId);
//     }
// }

