using MediatR;
using Teams.APP.Layer.DTOs;
namespace Teams.APP.Layer.FeatureTeam.GetTeam;

public sealed class GetTeamStatsQuery : IRequest<TeamStatsDto>
{
    public Guid Id { get;   init; }

    public GetTeamStatsQuery(Guid id) => Id = id;
}