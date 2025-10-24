using AutoMapper;
using MediatR;
using Teams.API.Layer.DTOs;
using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.CQRS.Queries;
using Teams.INFRA.Layer.Interfaces;

namespace Teams.APP.Layer.CQRS.Handlers;

public class GetTeamStatsQueryHandler(
    IMapper _mapper,
    IUnitOfWork _unitOfWork,
    ILogger<GetTeamStatsQueryHandler> _log
) : IRequestHandler<GetTeamStatsQuery, TeamStatsDto>
{
    public async Task<TeamStatsDto> Handle(
        GetTeamStatsQuery request,
        CancellationToken cancellationToken
    )
    {
        var team = await _unitOfWork.TeamRepository.GetById(cancellationToken, request.Id)
           ?? throw new HandlerException(
               404,
               $"Team with ID {request.Id} not found.",
               "Not Found",
               "Team resource not found"
           );
        var teamDto = _mapper.Map<TeamStatsDto>(team);
        return teamDto;
    }
}