using AutoMapper;
using Teams.CORE.Layer.Entities.TeamAggregate.InternalEntities;
using Teams.APP.Layer.DTOs;

namespace Teams.APP.Layer.Mappings;

public class ProjectProfile : Profile
{
    public ProjectProfile()
    {
        CreateMap<ProjectStateDto, VoState>().ConvertUsing(src => src.State);
        CreateMap<DetailDto, Detail>()
            .ConvertUsing(src => new Detail(
                src.ProjectName,
                src.ProjectStartDate,
                src.ProjectEndDate,
                src.VoState.State
            ));

        CreateMap<ProjectAssociationDto, ProjectAssociation>()
            .ConstructUsing(
                (dto, context) =>
                    new ProjectAssociation(
                        dto.ProjectId,
                        dto.TeamManagerId,
                        dto.TeamName,
                        context.Mapper.Map<List<Detail>>(dto.Details)
                    )
            );
    }
}
