using MediatR;
namespace Teams.APP.Layer.FeatureTeam.DeleteTeamByMember;
public record DeleteTeamByMemberCommand(Guid MemberId, string TeamName) : IRequest;