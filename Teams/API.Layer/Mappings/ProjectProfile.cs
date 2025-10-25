using AutoMapper;
using NodaTime.Extensions;
using Teams.CORE.Layer.Entities.TeamAggregate.InternalEntities;
using Teams.CORE.Layer.Entities.GeneralValueObjects;
using Teams.INFRA.Layer.ExternalServicesDtos;

namespace Teams.API.Layer.Mappings;

public class ProjectProfile : Profile
{
    public ProjectProfile()
    {
        CreateMap<ProjectStateDto, VoState>().ConvertUsing(src => src.State);
        CreateMap<DetailDto, Detail>()
            .ConvertUsing(src => new Detail(
                src.ProjectName,
                LocalizationDateTime.FromDateTimeUtc(src.ProjectStartDate),
                LocalizationDateTime.FromDateTimeUtc(src.ProjectEndDate),
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
