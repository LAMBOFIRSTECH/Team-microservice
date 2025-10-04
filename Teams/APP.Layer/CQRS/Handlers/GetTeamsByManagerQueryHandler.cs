using MediatR;
using Teams.API.Layer.DTOs;
using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.CQRS.Queries;
using Teams.CORE.Layer.Entities.TeamAggregate;

namespace Teams.APP.Layer.CQRS.Handlers;

public class GetTeamsByManagerQueryHandler(ITeamRepository teamRepository)
    : IRequestHandler<GetTeamsByManagerQuery, List<TeamRequestDto>>
{
    public async Task<List<TeamRequestDto>> Handle(
        GetTeamsByManagerQuery request,
        CancellationToken cancellationToken
    )
    {
        var teams = await teamRepository.GetTeamsByManagerIdAsync(
            request.TeamManagerId,
            cancellationToken
        );
        if (teams == null || teams.Count.Equals(0))
            throw new HandlerException(
                404,
                $"Team with Manager ID {request.TeamManagerId} not found.",
                "Not Found",
                "Team ressource not found"
            );
        var teamDtos = teams
            .Select(team => new TeamRequestDto(
                team.Id,
                team.TeamManagerId.Value,
                team.Name.Value,
                team.MembersIds.Select(m=> m.Value).ToHashSet(),
                request.IncludeMembers
            ))
            .ToList();
        return teamDtos;
    }
}
