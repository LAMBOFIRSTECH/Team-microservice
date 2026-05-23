using AutoMapper;
using MediatR;
using Teams.APP.Layer.DTOs;
using Teams.APP.Layer.Exceptions;
using Teams.APP.Layer.FeatureTeam.GetTeam;
using Teams.CORE.Layer.CoreInterfaces;
using Microsoft.Extensions.Logging;

namespace Teams.APP.Layer.FeatureTeam.GetTeam;
public class GetTeamStatsHandler(IMapper _mapper, IUnitOfWork _unitOfWork, ILogger<GetTeamStatsHandler> _log) : IRequestHandler<GetTeamStatsQuery, TeamStatsDto>
{
    public async Task<TeamStatsDto> Handle( GetTeamStatsQuery request, CancellationToken cancellationToken)
    {
        var team = await _unitOfWork.TeamRepository.GetByIdAsync(request.Id, cancellationToken)
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