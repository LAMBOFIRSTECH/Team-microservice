namespace Teams.CORE.Layer.CoreInterfaces;

public interface ITeamProjectLifeCycle
{
    Task RemoveProjects(CancellationToken ct);
    Task DeleteTeamProjectAsync(CancellationToken cancellationToken, Guid teamId);
    Task AddProjectToTeamAsync(string message);
    Task SuspendProjectAsync(string message);
    Task<DateTimeOffset?> GetNextProjectExpirationDate(CancellationToken cancellationToken = default);
}