using Hangfire;
using Serilog;
using Teams.APP.Layer.Exceptions;
using Teams.APP.Layer.Helpers;
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
            LogHelper.Info(
                "🚀 Planification du job Hangfire pour l'ajout d'un nouveau membre dans l'équipe",
                log
            );
            string jobId = TryScheduleJob(
                () =>
                    BackgroundJob.Schedule(
                        () => employeeService.AddTeamMemberIntoRedisCacheAsync(memberId),
                        TimeSpan.FromSeconds(10)
                    ),
                retryCount: 3,
                delay: TimeSpan.FromSeconds(5)
            );
            if (string.IsNullOrEmpty(jobId))
            {
                LogHelper.Error("❌ La planification du job a échoué. jobId est null.", log);
                throw new HangFireException(
                    500,
                    "Internal Error",
                    "❌ La planification du job a échoué",
                    "jobId est null."
                );
            }
            LogHelper.Info(
                "✅ Job Hangfire planifié avec succès pour l'ajout du membre dans l'équipe",
                log
            );
        }
        catch (Exception ex)
        {
            log.LogError(
                ex,
                "❌ Erreur lors de la planification du job Hangfire. Détails de l'exception"
            );
            LogHelper.Error(
                "❌ Erreur lors de la planification du job Hangfire. Détails de l'exception",
                log
            );
        }
    }

    [Queue("runner_operation_delete_new_member")]
    public void ScheduleDeleteTeamMemberAsync(Guid memberId, string teamName)
    {
        try
        {
            LogHelper.Info(
                "🚀 Planification du job Hangfire pour suppression du membre de l'équipe",
                log
            );
            string jobId = TryScheduleJob(
                () =>
                    BackgroundJob.Schedule(
                        () => employeeService.DeleteTeamMemberAsync(memberId, teamName),
                        TimeSpan.FromSeconds(10)
                    ),
                retryCount: 3,
                delay: TimeSpan.FromSeconds(5)
            );
            if (string.IsNullOrEmpty(jobId))
            {
                LogHelper.Error("❌ La planification du job a échoué. jobId est null.", log);
                throw new HangFireException(
                    500,
                    "Internal Error",
                    "❌ La planification du job a échoué",
                    "jobId est null."
                );
            }

            LogHelper.Info(
                "✅ Job Hangfire planifié avec succès pour la suppression du membre de l'équipe",
                log
            );
        }
        catch (Exception ex)
        {
            log.LogError(
                ex,
                "❌ Erreur lors de la planification du job Hangfire. Détails de l'exception"
            );
            LogHelper.Error(
                "❌ Erreur lors de la planification du job Hangfire. Détails de l'exception",
                log
            );
        }
    }

    [Queue("runner_operation_project")]
    public void ScheduleProjectAssociationAsync()
    {
        try
        {
            LogHelper.Info(
                "🚀 Planification du job Hangfire pour récupération des données dans le Microservice Projet",
                log
            );
            string jobId = TryScheduleJob(
                () =>
                    BackgroundJob.Schedule(
                        () => project.ManageTeamteamProjectAsync(),
                        TimeSpan.FromSeconds(10)
                    ),
                retryCount: 3,
                delay: TimeSpan.FromSeconds(5)
            );
            if (string.IsNullOrEmpty(jobId))
            {
                LogHelper.Error("❌ La planification du job a échoué. jobId est null.", log);
                throw new HangFireException(
                    500,
                    "Internal Error",
                    "❌ La planification du job a échoué",
                    "jobId est null."
                );
            }

            LogHelper.Info(
                "✅ Job Hangfire planifié avec succès pour la récupération des données dans le Microservice Projet",
                log
            );
        }
        catch (Exception ex)
        {
            log.LogError(
                ex,
                "❌ Erreur lors de la planification du job Hangfire. Détails de l'exception"
            );
            LogHelper.Error(
                "❌ Erreur lors de la planification du job Hangfire. Détails de l'exception",
                log
            );
        }
    }
}
