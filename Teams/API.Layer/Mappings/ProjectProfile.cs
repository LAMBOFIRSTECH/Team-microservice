using AutoMapper;
using Teams.CORE.Layer.ValueObjects;
using Teams.INFRA.Layer.ExternalServicesDtos;

namespace Teams.API.Layer.Mappings;

public class ProjectProfile : Profile
{
    public ProjectProfile()
    {
        CreateMap<ProjectStateDto, ProjectState>().ConvertUsing(src => src.State);

        CreateMap<ProjectAssociationDto, ProjectAssociation>()
            .ConstructUsing(dto => new ProjectAssociation(
                dto.TeamManagerId,
                dto.TeamName,
                dto.ProjectName,
                dto.ProjectStartDate,
                dto.ProjectEndDate,
                dto.ProjectState.State
            ));
    }
}
