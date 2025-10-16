using MediatR;
using Teams.API.Layer.DTOs;
using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.CQRS.Queries;
using Teams.APP.Layer.Helpers;
using Teams.APP.Layer.Interfaces;
using Teams.CORE.Layer.Entities.TeamAggregate;

namespace Teams.APP.Layer.CQRS.Handlers;

public class GetTeamQueryHandler(
    ITeamRepository teamRepository,
    IRedisCacheService redisCache,
    ILogger<GetTeamQueryHandler> log,
    IProjectService projectService
) : IRequestHandler<GetTeamQuery, TeamDetailsDto>
{
    public async Task<TeamDetailsDto> Handle(GetTeamQuery request, CancellationToken cancellationToken)
    {
        var team = await teamRepository.GetTeamByIdAsync(request.Id, cancellationToken);
        if (team is not null)
        {
            LogHelper.Info($"✅ Team with ID={request.Id} exist in database.", log);
            return projectService.BuildDto(team);
        }
        var archivedTeamDto = await redisCache.GetArchivedTeamFromRedisAsync(request.Id, cancellationToken);
        if (archivedTeamDto is not null)
        {
            Enum.TryParse(archivedTeamDto.State, out TeamState currentStatus);
            if (currentStatus == TeamState.Archived)
            {
                archivedTeamDto.State = $"Team {archivedTeamDto.Name} has been archived for 7 days. No more present in database";
                return archivedTeamDto;
            }
        }
        LogHelper.Error($"❌ Team with ID={request.Id} not found.", log);
        throw HandlerException.NotFound(
            title: "Not Found",
            statusCode: 404,
            message: $"Team with ID {request.Id} not found.",
            reason: "Resource not found"
        );
    }
}
