namespace Teams.INFRA.Layer.Interfaces;

public interface ITeamExpiryScheduler
{
    Task RescheduleAsync(CancellationToken ct = default);
    Task StartAsync(CancellationToken ct);
    Task StopAsync(CancellationToken ct);
}
