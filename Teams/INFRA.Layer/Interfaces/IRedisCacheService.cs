using Teams.INFRA.Layer.DTOs;

namespace Teams.INFRA.Layer.Interfaces;

public interface IRedisCacheService
{
    Task StoreArchivedTeamInRedisAsync(TeamDtoModels.TeamDetailsDto redisTeamDto, CancellationToken cancellationToken);
    Task<TeamDtoModels.TeamDetailsDto> GetArchivedTeamFromRedisAsync(Guid teamId, CancellationToken cancellationToken);
    Task StoreNewTeamMemberInformationsInRedisAsync(Guid memberId, string teamName, CancellationToken cancellationToken = default);
    Task<string> GetNewTeamMemberFromCacheAsync(Guid memberId, CancellationToken cancellationToken = default);
}
