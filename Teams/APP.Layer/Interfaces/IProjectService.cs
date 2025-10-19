using Teams.API.Layer.DTOs;
using Teams.CORE.Layer.Entities.TeamAggregate.InternalEntities;
using Teams.CORE.Layer.Entities.TeamAggregate;
namespace Teams.APP.Layer.Interfaces;

public interface IProjectService
{
    Task ManageTeamProjectAsync(Guid managerId, string teamName);
    Task<ProjectAssociation> GetProjectAssociationDataAsync(Guid? managerId, string teamName);
    Task SuspendProjectAsync(Guid managerId, string projectName);
    TeamDetailsDto BuildDto(Team team);
}
