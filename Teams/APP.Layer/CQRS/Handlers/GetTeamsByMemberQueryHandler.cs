using MediatR;
using Teams.API.Layer.DTOs;
using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.CQRS.Queries;
using Teams.CORE.Layer.Interfaces;

namespace Teams.APP.Layer.CQRS.Handlers;

public class GetTeamsByMemberQueryHandler(ITeamRepository teamRepository)
    : IRequestHandler<GetTeamsByMemberQuery, List<TeamRequestDto>>
{
    public async Task<List<TeamRequestDto>> Handle(
        GetTeamsByMemberQuery request,
        CancellationToken cancellationToken
    )
    {
        var teams = await teamRepository.GetTeamsByMemberIdAsync(
            request.MemberId,
            cancellationToken
        );
        if (teams == null || teams.Count.Equals(0))
            throw new HandlerException(
                404,
                $"Team with Member ID {request.MemberId} not found.",
                "Not Found",
                "Team ressource not found"
            );
        var teamDtos = teams
            .Select(team => new TeamRequestDto(
                team.Id,
                team.TeamManagerId.Value,
                team.Name.Value,
                team.MembersIds.Select(m=>m.Value).ToHashSet(),
                request.IncludeMembers
            ))
            .ToList();
        return teamDtos;
    }
}
