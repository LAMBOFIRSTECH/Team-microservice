using Teams.API.Layer.DTOs;
using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.CORE.Layer.Entities.TeamAggregate.InternalEntities;

namespace Teams.APP.Layer.Interfaces;

public interface ITeamProjectLifeCycle
{
    Task RemoveProjects(CancellationToken ct);
    Task DeleteTeamProjectAsync(CancellationToken cancellationToken, Guid teamId);
    Task AddProjectToTeamAsync(string message);
    Task SuspendProjectAsync(string message);
    Task<DateTimeOffset?> GetNextProjectExpirationDate(CancellationToken cancellationToken = default);
    TeamDetailsDto BuildDto(Team team);
}