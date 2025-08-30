using AutoMapper;
using Teams.API.Layer.DTOs;
using Teams.CORE.Layer.Entities;

namespace Teams.API.Layer.Mappings;

public class TeamProfile : Profile
{
    public TeamProfile()
    {
        CreateMap<Team, TeamDto>();
        CreateMap<Team, TeamRequestDto>();
        CreateMap<Team, ChangeManagerDto>();
        CreateMap<Team, TeamDetailsDto>()
            .ForMember(
                dest => dest.MembersId,
                opt => opt.MapFrom(src => src.MembersIds.Select(m => m.Equals(Guid.Empty)).ToList())
            );
    }
}
