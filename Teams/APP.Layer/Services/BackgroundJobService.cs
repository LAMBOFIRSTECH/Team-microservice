using Hangfire;
using Teams.APP.Layer.Exceptions;
using Teams.APP.Layer.Interfaces;

namespace Teams.APP.Layer.Services;

public class BackgroundJobService(
    IEmployeeService employeeService,
    ProjectService project,
    ILogger<BackgroundJobService> log
) : IBackgroundJobService
{
    public string TryScheduleJob(Func<string> scheduleJobAction, int retryCount, TimeSpan delay)
    {
        string? jobId = null;
        for (int i = 0; i < retryCount; i++)
        {
            jobId = scheduleJobAction();
            if (!string.IsNullOrEmpty(jobId))
                break;

            Thread.Sleep(delay);
        }
        return jobId!;
    }

    [Queue("runner_operation_add_new_member")]
    public void ScheduleAddTeamMemberAsync(Guid memberId)
    {
        try
        {
            log.LogInformation(
                "🚀 Planification du job Hangfire pour récupération des données dans le Microservice Employees"
            );
            string jobId = TryScheduleJob(
                () =>
                    BackgroundJob.Schedule(
                        () => employeeService.AddTeamMemberIntoRedisCacheAsync(memberId),
                        TimeSpan.FromSeconds(10)
                    ),
                retryCount: 1,
                delay: TimeSpan.FromSeconds(1)
            );
            if (string.IsNullOrEmpty(jobId))
            {
                throw new HangFireException(
                    500,
                    "Internal Error",
                    "❌ La planification du job a échoué",
                    "jobId est null."
                );
            }
            log.LogInformation("✅ Job Hangfire planifié avec succès. Job ID : {jobId}", jobId);
        }
        catch (Exception ex)
        {
            log.LogError(
                ex,
                "❌ Erreur lors de la planification du job Hangfire. Détails de l'exception"
            );
        }
    }

    [Queue("runner_operation_delete_new_member")]
    public void ScheduleDeleteTeamMemberAsync(Guid memberId, string teamName)
    {
        try
        {
            log.LogInformation(
                "🚀 Planification du job Hangfire pour récupération des données dans le Microservice Employees"
            );
            string jobId = TryScheduleJob(
                () =>
                    BackgroundJob.Schedule(
                        () => employeeService.DeleteTeamMemberAsync(memberId, teamName),
                        TimeSpan.FromSeconds(10)
                    ),
                retryCount: 2,
                delay: TimeSpan.FromSeconds(1)
            );
            if (string.IsNullOrEmpty(jobId))
            {
                throw new HangFireException(
                    500,
                    "Internal Error",
                    "❌ La planification du job a échoué",
                    "jobId est null."
                );
            }
            log.LogInformation("✅ Job Hangfire planifié avec succès. Job ID : {jobId}", jobId);
        }
        catch (Exception ex)
        {
            log.LogError(
                ex,
                "❌ Erreur lors de la planification du job Hangfire. Détails de l'exception"
            );
        }
    }

    [Queue("runner_operation_project")]
    public void ScheduleProjectAssociationAsync()
    {
        try
        {
            log.LogInformation(
                "🚀 Planification du job Hangfire pour récupération des données dans le Microservice Projet"
            );
            string jobId = TryScheduleJob(
                () =>
                    BackgroundJob.Schedule(
                        () => project.ManageTeamteamProjectAsync(),
                        TimeSpan.FromSeconds(10)
                    ),
                retryCount: 2,
                delay: TimeSpan.FromSeconds(1)
            );
            if (string.IsNullOrEmpty(jobId))
            {
                throw new HangFireException(
                    500,
                    "Internal Error",
                    "❌ La planification du job a échoué",
                    "jobId est null."
                );
            }
            log.LogInformation("✅ Job Hangfire planifié avec succès. Job ID : {jobId}", jobId);
        }
        catch (Exception ex)
        {
            log.LogError(
                ex,
                "❌ Erreur lors de la planification du job Hangfire. Détails de l'exception"
            );
        }
    }
}
