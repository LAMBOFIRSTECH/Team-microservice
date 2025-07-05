using System;
using Hangfire;

namespace Teams.APP.Layer.Services;

public interface IBackgroundJobService
{
    Task ScheduleAddTeamMemberAsync(Guid employeeId);
    Task ScheduleProjectAssociationAsync(Guid employeeId);
}

public class BackgroundJobService : IBackgroundJobService
{
    public Task ScheduleAddTeamMemberAsync(Guid employeeId)
    {
        return Task.Run(() =>
        {
            // Ajouter un job Hangfire pour l'ajout d'un membre de l'équipe
            BackgroundJob.Enqueue(() => AddTeamMemberAsync(employeeId));
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

    public async Task AddTeamMemberAsync(Guid employeeId)
    {
        // La logique d'ajout d'un membre à l'équipe
        await Task.Delay(1000); // Simule une tâche asynchrone
    }

    public async Task AssociateProjectAsync(Guid employeeId)
    {
        // La logique d'association du projet à un employé
        await Task.Delay(1000); // Simule une tâche asynchrone
    }
}
