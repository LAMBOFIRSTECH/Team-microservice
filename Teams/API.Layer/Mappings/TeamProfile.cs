using AutoMapper;
using Teams.API.Layer.DTOs;
using Teams.CORE.Layer.Entities;
namespace Teams.API.Layer.Mappings;

public class TeamProfile : Profile
{
    public TeamProfile()
    {
        CreateMap<Team, TeamDto>()
            .ForMember(dest => dest.TeamManagerId,
                       opt => opt.MapFrom(src => src.TeamManagerId.Value))
            .ForMember(dest => dest.MembersId,
                       opt => opt.MapFrom(src => src.MembersIds.Select(m => m.Value)))
            .ForMember(dest => dest.Name,
                       opt => opt.MapFrom(src => src.Name.Value));

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
        .ForMember(dest => dest.TeamCreationDate, opt => opt.MapFrom(src => src.TeamCreationDate.ToString("dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture)))
        .ForMember(dest => dest.TeamExpirationDate, opt => opt.MapFrom(src => src.ExpirationDate.ToString("dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture)))
        .ForMember(dest => dest.ActiveProject, opt => opt.MapFrom(src => src.MembersIds.Any() && src.ActiveAssociatedProject))
        .ForMember(dest => dest.State, opt => opt.MapFrom(src => Enum.GetName(src.State.GetType(), src.State)));
        // .ForMember(dest => dest.ProjectNames, opt => opt.MapFrom(src => src.ProjectAssociations.SelectMany(pa => pa.Details)))
    }
}
