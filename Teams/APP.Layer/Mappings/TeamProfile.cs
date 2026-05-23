using AutoMapper;
using Teams.APP.Layer.DTOs.Output;
using Teams.APP.Layer.DTOs;
using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.CORE.Layer.Entities.TeamAggregate.TeamDtos;
using Teams.CORE.Layer.Entities.TeamAggregate.TeamExtensionMethods;
namespace Teams.APP.Layer.Mappings;
public class TeamProfile : Profile
{
  public TeamProfile()
  {
    CreateMap<TeamDataForDto, TeamDtoModels.TeamDto>()
     .ForCtorParam("Id", opt => opt.MapFrom(src => src.Id))
     .ForCtorParam("Name", opt => opt.MapFrom(src => src.Name ?? string.Empty))
     .ForCtorParam("TeamManagerId", opt => opt.MapFrom(src => src.TeamManagerId))
     .ForCtorParam("MembersIds", opt => opt.MapFrom(src => src.MembersIds ?? new List<Guid>()));


    CreateMap<TeamDataForDto, TeamDtoModels.TeamDetailsDto>()
    .ConvertUsing((src, context) =>
    {
      var details = src.Project != null
          ? src.Project.Details.Select(d => d.ProjectName).ToList()
          : new List<string>();

      return new TeamDtoModels.TeamDetailsDto(
          src.Id,
          src.Name,
          src.TeamManagerId,
          src.TeamCreationDate.ToString("dd-MM-yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
          src.TeamExpirationDate.ToString("dd-MM-yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
          src.MembersIds.Any() && src.Project?.HasActiveProject() == true,
          details,
          src.State
      );
    });


    CreateMap<Team, TeamRequestDto>()
         .ForMember(dest => dest.TeamManagerId,
                   opt => opt.MapFrom(src => src.TeamManagerId.Value))
        .ForMember(dest => dest.MembersId,
                   opt => opt.MapFrom(src => src.MembersIds.Select(m => m.Value)))
        .ForMember(dest => dest.Name,
                   opt => opt.MapFrom(src => src.Name.Value));

    CreateMap<TeamDataForDto, ChangeManagerDto>()
       .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
       .ForMember(dest => dest.OldTeamManagerId, opt => opt.MapFrom(src => src.OldTeamManagerId));

    CreateMap<TeamDataForDto, TeamStatsDto>()
       .ForMember(dest => dest.TauxTurnOver, opt => opt.MapFrom(src => src.TauxTurnOver))
       .ForMember(dest => dest.AverageProductivity, opt => opt.MapFrom(src => src.AverageProductivity))
       .ForMember(dest => dest.LastActivityDate, opt => opt.MapFrom(src => src.LastActivityDate.ToString("dd-MM-yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture)));

  }
}
