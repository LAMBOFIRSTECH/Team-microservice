namespace Teams.INFRA.Layer.Interfaces;

public interface ITeamMaturityScheduler
{
    Task RescheduleAsync(CancellationToken ct = default);
    Task StartAsync(CancellationToken ct=default);
    Task StopAsync(CancellationToken ct=default);
}
