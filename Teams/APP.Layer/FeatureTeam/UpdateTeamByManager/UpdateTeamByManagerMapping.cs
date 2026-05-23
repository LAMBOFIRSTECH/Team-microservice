using AutoMapper;
namespace Teams.APP.Layer.FeatureTeam.UpdateTeamByManager;
public sealed class UpdateTeamByManagerMapping : Profile
{
    public UpdateTeamByManagerMapping()
    {
        CreateMap<UpdateTeamByManagerRequest, UpdateTeamByManagerCommand>()
         .ForCtorParam("TeamName", opt => opt.MapFrom(src => src.TeamName))
         .ForCtorParam("OldTeamManagerId", opt => opt.MapFrom(src => src.OldTeamManagerId))
         .ForCtorParam("NewTeamManagerId", opt => opt.MapFrom(src => src.NewTeamManagerId))
         .ForCtorParam("ContratType", opt => opt.MapFrom(src => src.ContratType));

    }
}
