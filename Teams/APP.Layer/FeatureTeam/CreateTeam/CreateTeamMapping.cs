using AutoMapper;
namespace Teams.APP.Layer.FeatureTeam.CreateTeam;
public sealed class CreateTeamMapping : Profile
{
    public CreateTeamMapping()
    {
        CreateMap<CreateTeamRequest, CreateTeamCommand>()
        .ForCtorParam("Id", opt => opt.MapFrom(src => src.Id))
        .ForCtorParam("Name", opt => opt.MapFrom(src => src.Name ?? string.Empty))
        .ForCtorParam("TeamManagerId", opt => opt.MapFrom(src => src.TeamManagerId))
        .ForCtorParam("MembersIds", opt => opt.MapFrom(src => src.MembersIds));
    }
}
