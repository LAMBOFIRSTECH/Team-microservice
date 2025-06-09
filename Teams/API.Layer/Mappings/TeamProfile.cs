using AutoMapper;
using Teams.CORE.Layer.Entities;
using Teams.API.Layer.DTOs;
using Teams.APP.Layer.CQRS.Commands;
namespace Teams.API.Layer.Mappings;
public class TeamProfile : Profile
{
    public TeamProfile()
    {
        CreateMap<Team, TeamDto>();
        CreateMap<TeamDto, Team>();
        CreateMap<CreateTeamCommand, Team>();
    }
}