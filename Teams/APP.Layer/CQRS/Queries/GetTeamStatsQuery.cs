using MediatR;
using Teams.API.Layer.DTOs;
namespace Teams.APP.Layer.CQRS.Queries;

public class GetTeamStatsQuery : IRequest<TeamStatsDto>
{
    public Guid Id { get; }

    public GetTeamStatsQuery(Guid id) => Id = id;
}