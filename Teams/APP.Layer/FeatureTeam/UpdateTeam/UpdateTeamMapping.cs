using AutoMapper;
namespace Teams.APP.Layer.FeatureTeam.UpdateTeam;
public sealed class UpdateTeamMapping : Profile
{
    public UpdateTeamMapping()
    {
        CreateMap<UpdateTeamRequest, UpdateTeamCommand>()
        .ForCtorParam("Id", opt => opt.MapFrom(src => src.Id))
        .ForCtorParam("Name", opt => opt.MapFrom(src => src.Name ?? string.Empty))
        .ForCtorParam("TeamManagerId", opt => opt.MapFrom(src => src.TeamManagerId))
        .ForCtorParam("MembersIds", opt => opt.MapFrom(src => src.MembersIds));
    }
}
