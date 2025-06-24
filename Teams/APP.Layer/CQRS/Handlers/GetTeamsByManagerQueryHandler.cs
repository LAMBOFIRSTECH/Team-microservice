using MediatR;
using Teams.API.Layer.DTOs;
using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.CQRS.Queries;
using Teams.CORE.Layer.Interfaces;

namespace Teams.APP.Layer.CQRS.Handlers;

public class GetTeamsByManagerQueryHandler
    : IRequestHandler<GetTeamsByManagerQuery, List<TeamRequestDto>>
{
    private readonly ITeamRepository teamRepository;

    public GetTeamsByManagerQueryHandler(ITeamRepository teamRepository)
    {
        this.teamRepository = teamRepository;
    }

    public async Task<List<TeamRequestDto>> Handle(
        GetTeamsByManagerQuery request,
        CancellationToken cancellationToken
    )
    {
        var teams = await teamRepository.GetTeamsByManagerIdAsync(request.TeamManagerId);
        if (teams == null || teams.Count.Equals(0))
            throw new HandlerException(
                404,
                $"Team with Manager ID {request.TeamManagerId} not found.",
                "Not Found",
                "Team ressource not found"
            );

        foreach (var team in teams)
        {
            if (team.MemberId == null || team.MemberId.Count == 0)
            {
                team.MemberId = new List<Guid>();
            }
        }
        var teamDtos = teams
            .Select(team => new TeamRequestDto(
                team.Id,
                team.TeamManagerId,
                team.Name,
                request.IncludeMembers,
                team.MemberId
            ))
            .ToList();
        return teamDtos;
    }
}
