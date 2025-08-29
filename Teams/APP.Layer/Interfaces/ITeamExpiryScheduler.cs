using System;
using Teams.CORE.Layer.Entities;

namespace Teams.APP.Layer.Interfaces;

public interface ITeamExpiryScheduler
{
    Task RescheduleAsync(CancellationToken ct = default);

    // Task RescheduleAsync(Team team, CancellationToken ct = default);

    Task StartAsync(CancellationToken ct);
    Task StopAsync(CancellationToken ct);
}
