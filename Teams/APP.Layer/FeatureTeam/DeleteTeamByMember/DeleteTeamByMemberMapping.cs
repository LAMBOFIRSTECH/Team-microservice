using AutoMapper;
namespace Teams.APP.Layer.FeatureTeam.DeleteTeamByMember;
public sealed class DeleteTeamByMemberMapping : Profile
{
    public DeleteTeamByMemberMapping()
    {

    CreateMap<DeleteTeamByMemberRequest, DeleteTeamByMemberCommand>()
     .ForCtorParam("Name", opt => opt.MapFrom(src => src.TeamName ?? string.Empty))
     .ForCtorParam("MembersIds", opt => opt.MapFrom(src => src.MemberId));
    }
}
