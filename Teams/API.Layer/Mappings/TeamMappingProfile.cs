using AutoMapper;
using Teams.CORE.Layer.Entities;
using Teams.API.Layer.DTOs;
namespace Teams.API.Layer.Mappings;
public class TeamMappingProfile : Profile
{
    public TeamMappingProfile()
    {
        CreateMap<Team, TeamDto>();
    }
}
