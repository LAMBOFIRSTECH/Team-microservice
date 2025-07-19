using System;

namespace Teams.APP.Layer.Interfaces;

public interface IRedisCacheService
{
    Task StoreNewTeamMemberInformationsInRedisAsync(Guid memberId, string teamName);
    Task<string> GetNewTeamMemberFromCacheAsync(Guid memberId);
}
