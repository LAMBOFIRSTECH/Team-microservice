using AutoMapper;
using Teams.API.Layer.DTOs;
using Teams.CORE.Layer.Entities.TeamAggregate;
namespace Teams.API.Layer.Mappings;

public class TeamProfile : Profile
{
  public TeamProfile()
  {
    CreateMap<Team, TeamDto>()
      .ForMember(dest => dest.Name,
       opt => opt.MapFrom(src => src.Name != null ? src.Name.Value : string.Empty))
     .ForMember(dest => dest.TeamManagerId,
       opt => opt.MapFrom(src => src.TeamManagerId != null ? src.TeamManagerId.Value : Guid.Empty))
     .ForMember(dest => dest.MembersIds,
       opt => opt.MapFrom(src => src.MembersIds != null
                                 ? src.MembersIds.Select(m => m.Value)
                                 : new List<Guid>()));


    CreateMap<Team, TeamRequestDto>()
         .ForMember(dest => dest.TeamManagerId,
                   opt => opt.MapFrom(src => src.TeamManagerId.Value))
        .ForMember(dest => dest.MembersId,
                   opt => opt.MapFrom(src => src.MembersIds.Select(m => m.Value)))
        .ForMember(dest => dest.Name,
                   opt => opt.MapFrom(src => src.Name.Value));

    CreateMap<Team, ChangeManagerDto>();
    CreateMap<Team, TeamStatsDto>();
    CreateMap<Team, TeamDetailsDto>()
    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Value))
    .ForMember(dest => dest.TeamManagerId, opt => opt.MapFrom(src => src.TeamManagerId.Value))
    .ForMember(dest => dest.TeamCreationDate, opt => opt.MapFrom(src => src.TeamCreationDate.Value.ToString("dd-MM-yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture)))
    .ForMember(dest => dest.TeamExpirationDate, opt => opt.MapFrom(src => src.TeamExpirationDate.Value.ToString("dd-MM-yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture)))
    .ForMember(dest => dest.HasAnyProject, opt => opt.MapFrom(src => src.MembersIds.Any() && src.Project != null && src.Project.HasActiveProject() ? true : false))
    .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State));
  }
}
