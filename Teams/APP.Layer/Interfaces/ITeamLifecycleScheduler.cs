namespace Teams.APP.Layer.Interfaces;

public interface ITeamLifecycleScheduler
{
    Task RescheduleAsync(CancellationToken ct = default);
    Task StartAsync(CancellationToken ct);
    Task StopAsync(CancellationToken ct);
}
