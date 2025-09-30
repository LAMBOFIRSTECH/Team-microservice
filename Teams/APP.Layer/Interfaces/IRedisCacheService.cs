using Teams.API.Layer.DTOs;
using Teams.CORE.Layer.Entities;

namespace Teams.APP.Layer.Interfaces;

public interface IRedisCacheService
{
    Task StoreArchivedTeamInRedisAsync(TeamDetailsDto redisTeamDto, CancellationToken cancellationToken);
    Task<TeamDetailsDto> GetArchivedTeamFromRedisAsync(Guid teamId, CancellationToken cancellationToken);
    Task StoreNewTeamMemberInformationsInRedisAsync(Guid memberId, string teamName, CancellationToken cancellationToken = default);
    Task<string> GetNewTeamMemberFromCacheAsync(Guid memberId, CancellationToken cancellationToken = default);
}
