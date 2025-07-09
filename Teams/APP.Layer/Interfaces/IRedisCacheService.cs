using System;

namespace Teams.APP.Layer.Interfaces;

public interface IRedisCacheService
{
    void StoreNewTeamMemberInformationsInRedis(Guid memberId, string teamName);
    void GetNewTeamMemberFromCacheAsync();
}
