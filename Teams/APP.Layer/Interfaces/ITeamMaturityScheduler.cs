namespace Teams.APP.Layer.Interfaces;

public interface ITeamMaturityScheduler
{
    Task RescheduleAsync(CancellationToken ct = default);
    Task StartAsync(CancellationToken ct);
    Task StopAsync(CancellationToken ct);
}
