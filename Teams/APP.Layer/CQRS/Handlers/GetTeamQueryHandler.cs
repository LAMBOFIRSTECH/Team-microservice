using MediatR;
using Teams.API.Layer.DTOs;
using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.CQRS.Queries;
using Teams.APP.Layer.Helpers;
using Teams.APP.Layer.Interfaces;
using Teams.CORE.Layer.CommonExtensions;
using Teams.INFRA.Layer.Interfaces;

namespace Teams.APP.Layer.CQRS.Handlers;

public class GetTeamQueryHandler(IRedisCacheService _redisCache, IUnitOfWork _unitOfWork, ITeamProjectLifeCycle _teamProjectLifeCycle, ILogger<GetTeamQueryHandler> _log) : IRequestHandler<GetTeamQuery, TeamDetailsDto>
{
    public async Task<TeamDetailsDto> Handle(GetTeamQuery request, CancellationToken cancellationToken)
    {
        var team = await _unitOfWork.TeamRepository.GetById(cancellationToken, request.Id);
        if (team is not null)
        {
            LogHelper.Info($"✅ Team with ID={request.Id} exist in database.", _log);
            return await _teamProjectLifeCycle.BuildDto(team);
        }
        var archivedTeamDto = await _redisCache.GetArchivedTeamFromRedisAsync(request.Id, cancellationToken);
        if (archivedTeamDto is not null)
        {
            if (archivedTeamDto.State.UseTeamArchivedState())
            {
                archivedTeamDto.State = $"Team {archivedTeamDto.Name} has been archived for 7 days. No more present in database";
                return archivedTeamDto;
            }
        }
        LogHelper.Error($"❌ Team with ID={request.Id} not found.", _log);
        throw HandlerException.NotFound(title: "Not Found", statusCode: 404, message: $"Team with ID {request.Id} not found.", reason: "Resource not found");
    }
}
