using System;

namespace Teams.APP.Layer.Interfaces;

public interface ITeamMaturityScheduler
{
    Task RescheduleMaturityTeamAsync(CancellationToken ct = default);
    Task StartAsync(CancellationToken ct);
    Task StopAsync(CancellationToken ct);
}
