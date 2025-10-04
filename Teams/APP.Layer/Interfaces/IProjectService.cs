using Teams.CORE.Layer.Entities.GeneralValueObjects;
namespace Teams.APP.Layer.Interfaces;

public interface IProjectService
{
    Task ManageTeamProjectAsync(Guid managerId, string teamName);
    Task<ProjectAssociation> GetProjectAssociationDataAsync(Guid? managerId, string teamName);
    Task SuspendProjectAsync(Guid managerId, string projectName);
}
