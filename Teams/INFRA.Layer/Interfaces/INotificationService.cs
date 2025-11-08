namespace Teams.INFRA.Layer.Interfaces;

public interface INotificationService
{
    Task NotifyTeamArchived(Guid Id, CancellationToken ct);
}
