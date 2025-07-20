using Teams.CORE.Layer.ValueObjects;
using Teams.INFRA.Layer.ExternalServicesDtos;

namespace Teams.API.Layer.Mappings;

public class ProjectAssociationMapper
{
    public static class TeamProjectAssociation
    {
        public static ProjectAssociation ToDomain(ProjectAssociationDto dto)
        {
            return new ProjectAssociation(
                dto.TeamManagerId,
                dto.TeamName,
                dto.ProjectStartDate,
                new Teams.CORE.Layer.ValueObjects.ProjectState()
            );
        }
    }
}
