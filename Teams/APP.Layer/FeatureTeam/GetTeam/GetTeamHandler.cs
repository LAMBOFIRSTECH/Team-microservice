using MediatR;
using Teams.APP.Layer.Helpers;
using Teams.CORE.Layer.Entities.TeamAggregate.TeamExtensionMethods;
using AutoMapper;
using Teams.CORE.Layer.CoreInterfaces;
using Microsoft.Extensions.Logging;
using Teams.APP.Layer.DTOs.Output;
using Teams.APP.Layer.Exceptions;

namespace Teams.APP.Layer.FeatureTeam.GetTeam;

//Application (APP) ne doit effectivement voir que le CORE. Il ne doit jamais avoir de référence vers l'API ni vers l'INFRA. C'est une règle d'or pour maintenir une architecture propre et éviter les dépendances cycliques. L'APP doit uniquement interagir avec le CORE pour exécuter la logique métier, tandis que l'INFRA gère les détails de l'implémentation, comme l'accès aux données ou les services externes. En respectant cette séparation, on garantit que chaque couche reste indépendante et facilement testable.
public class GetTeamHandler(IRedisCacheService _redisCache, IMapper _mapper, IUnitOfWork _unitOfWork, ITeamProjectLifeCycle _teamProjectLifeCycle, ILogger<GetTeamQueryHandler> _log) : IRequestHandler<GetTeamQuery, TeamDtoModels.TeamDetailsDto>
{
    public async Task<TeamDtoModels.TeamDetailsDto> Handle(GetTeamQuery request, CancellationToken cancellationToken)
    {
        var team = await _unitOfWork.TeamRepository.GetByIdAsync(request.Id, cancellationToken);
        if (team is not null)
        {
            LogHelper.Info($"✅ Team with ID={request.Id} exist in database.", _log);
            return _mapper.Map<TeamDtoModels.TeamDetailsDto>(team.GetTeamDataForDto());
        }
        var archivedTeamDto = await _redisCache.GetArchivedTeamFromRedisAsync(request.Id, cancellationToken);
        if (archivedTeamDto is not null)
        {
            if (archivedTeamDto.State.UseTeamArchivedState())
            {
                var updatedArchivedTeamDto = archivedTeamDto with
                {
                    State = $"Team {archivedTeamDto.Name} has been archived for 7 days. No more present in database"
                };
                return updatedArchivedTeamDto;
            }
        }
        LogHelper.Error($"❌ Team with ID={request.Id} not found.", _log);
        throw HandlerException.NotFound(title: "Not Found", statusCode: 404, message: $"Team with ID {request.Id} not found.", reason: "Resource not found");
    }
}
