using System;
using Hangfire;
using Teams.APP.Layer.Interfaces;

namespace Teams.APP.Layer.Services;

public class BackgroundJobService(EmployeeService employeeService) : IBackgroundJobService
{
    public Task ScheduleAddTeamMemberAsync(Guid memberId)
    {
        return Task.Run(() =>
        {
            // Ajouter un job Hangfire pour l'ajout d'un membre de l'équipe
            BackgroundJob.Enqueue(() => employeeService.AddTeamMemberAsync(memberId));
        });
    }

    public Task ScheduleDeleteTeamMemberAsync(Guid memberId, string teamName)
    {
        return Task.Run(() =>
        {
            // Ajouter un job Hangfire pour l'ajout d'un membre de l'équipe
            BackgroundJob.Enqueue(() => employeeService.DeleteTeamMemberAsync(memberId, teamName));
        });
    }

    public Task ScheduleProjectAssociationAsync(Guid employeeId)
    {
        return Task.Run(() =>
        {
            // Ajouter un job Hangfire pour l'association d'un projet
            BackgroundJob.Enqueue(() => AssociateProjectAsync(employeeId));
        });
    }

    public async Task AssociateProjectAsync(Guid employeeId)
    {
        // La logique d'association du projet à un employé
        await Task.Delay(1000); // Simule une tâche asynchrone
    }
}
