using Teams.CORE.Layer.Entities.TeamAggregate.InternalEntities;
namespace Teams.CORE.Layer.CoreInterfaces;
public interface IProjectProvider
{
    Task<ProjectAssociation> RetrieveProjectForTeamAsync(Guid projectId);
}