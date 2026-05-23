using AutoMapper;
namespace Teams.APP.Layer.FeatureTeam.DeleteTeam;
public sealed class DeleteTeamMapping : Profile
{
    public DeleteTeamMapping()
    {
        CreateMap<DeleteTeamRequest, DeleteTeamCommand>()
            .ForCtorParam("Id", opt => opt.MapFrom(src => src.Id))
            .ForCtorParam("Name", opt => opt.MapFrom(src => src.Name ?? string.Empty));
    }
}
