using AutoMapper;
using MediatR;
using Teams.API.Layer.DTOs;
using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.CQRS.Queries;
using Teams.APP.Layer.Helpers;
using Teams.CORE.Layer.Entities;
using Teams.CORE.Layer.Interfaces;

namespace Teams.APP.Layer.CQRS.Handlers;

public class GetTeamStatsQueryHandler(
    ITeamRepository teamRepository,
    IMapper mapper,
    ILogger<GetTeamStatsQueryHandler> log
) : IRequestHandler<GetTeamStatsQuery, TeamStatsDto>
{
    public async Task<TeamStatsDto> Handle(
        GetTeamStatsQuery request,
        CancellationToken cancellationToken
    )
    {
        var team = await teamRepository.GetTeamByIdAsync(request.Id, cancellationToken)
           ?? throw new HandlerException(
               404,
               $"Team with ID {request.Id} not found.",
               "Not Found",
               "Team ressource not found"
           );
        var teamDto = mapper.Map<TeamStatsDto>(team);
        return teamDto;
    }
}