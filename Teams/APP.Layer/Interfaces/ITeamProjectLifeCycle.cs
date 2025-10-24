using Teams.API.Layer.DTOs;
using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.CORE.Layer.Entities.TeamAggregate.InternalEntities;

namespace Teams.APP.Layer.Interfaces;

public interface ITeamProjectLifeCycle
{
    Task RemoveProjects(CancellationToken ct);
    Task DeleteTeamProjectAsync(CancellationToken cancellationToken, Guid teamId);
    Task AddProjectToTeamAsync(Team team, ProjectAssociation project);
    TeamDetailsDto BuildDto(Team team);
}