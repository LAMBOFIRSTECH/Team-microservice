using AutoMapper;
using Teams.API.Layer.DTOs;
using Teams.APP.Layer.CQRS.Commands;
using Teams.CORE.Layer.Entities;

namespace Teams.API.Layer.Mappings;

public class TeamProfile : Profile
{
    public TeamProfile()
    {
        CreateMap<Team, TeamDto>();
        CreateMap<TeamDto, Team>();
        CreateMap<Team, TeamRequestDto>();
        CreateMap<TeamRequestDto, Team>();
        CreateMap<CreateTeamCommand, Team>();
        CreateMap<UpdateTeamCommand, Team>();
    }
}
