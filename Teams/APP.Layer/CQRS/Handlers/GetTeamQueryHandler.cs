using AutoMapper;
using MediatR;
using Teams.API.Layer.DTOs;
using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.CQRS.Queries;
using Teams.APP.Layer.Helpers;
using Teams.CORE.Layer.Interfaces;

namespace Teams.APP.Layer.CQRS.Handlers;

public class GetTeamQueryHandler(
    ITeamRepository teamRepository,
    IMapper mapper,
    ILogger<GetTeamQueryHandler> log
) : IRequestHandler<GetTeamQuery, TeamDto>
{
    public async Task<TeamDto> Handle(GetTeamQuery request, CancellationToken cancellationToken)
    {
        var team =
            await teamRepository.GetTeamByIdAsync(request.Id)!
            ?? throw new HandlerException(
                404,
                $"Team with ID {request.Id} not found.",
                "Not Found",
                "Team ressource not found"
            );
        LogHelper.Info($"✅ Team state is {team.State} and {team.StateMappings[team.State]}.", log);
        var teamDto = mapper.Map<TeamDto>(team);
        return teamDto;
    }
}
