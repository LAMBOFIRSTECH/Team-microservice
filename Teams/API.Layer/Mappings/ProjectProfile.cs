using AutoMapper;
using Teams.CORE.Layer.ValueObjects;
using Teams.INFRA.Layer.ExternalServicesDtos;

namespace Teams.API.Layer.Mappings;

public class ProjectProfile : Profile
{
    public ProjectProfile()
    {
        CreateMap<ProjectStateDto, ProjectState>().ConvertUsing(src => src.State);
        CreateMap<DetailDto, Detail>()
            .ConvertUsing(src => new Detail(
                src.ProjectName,
                src.ProjectStartDate,
                src.ProjectEndDate,
                src.ProjectState.State
            ));

        CreateMap<ProjectAssociationDto, ProjectAssociation>()
            .ConstructUsing(
                (dto, context) =>
                    new ProjectAssociation(
                        dto.TeamManagerId,
                        dto.TeamName,
                        context.Mapper.Map<List<Detail>>(dto.Details)
                    )
            );
    }
}
