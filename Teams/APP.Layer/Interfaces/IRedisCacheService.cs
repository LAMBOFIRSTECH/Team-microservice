using Teams.CORE.Layer.Entities;

namespace Teams.APP.Layer.Interfaces;

public interface IRedisCacheService
{
    Task StoreArchivedTeamInRedisAsync(Team team, CancellationToken cancellationToken);
    Task StoreNewTeamMemberInformationsInRedisAsync(Guid memberId, string teamName);
    Task<string> GetNewTeamMemberFromCacheAsync(Guid memberId);
}
