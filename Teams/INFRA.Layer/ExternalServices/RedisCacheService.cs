using System.Reflection.Metadata.Ecma335;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Teams.APP.Layer.Helpers;
using Teams.APP.Layer.Interfaces;

namespace Teams.INFRA.Layer.ExternalServices;

public class RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> log)
    : IRedisCacheService
{
    private string GetKey(string key) => $"DevCache:{key}";

    // HGET DevCache:12345678-90ab-cdef-1234-567890abcdef data

    public async Task StoreNewTeamMemberInformationsInRedisAsync(Guid memberId, string teamName)
    {
        var cacheKey = GetKey(memberId.ToString());

        var cachedData = await cache.GetStringAsync(cacheKey);

        if (cachedData is not null)
        {
            LogHelper.Error($"Key already exists in Redis: {cacheKey}", log);
            throw new InvalidOperationException();
        }

        var jsonObject = new Dictionary<string, object>
        {
            { "Id member", memberId },
            { "Team Name", teamName },
        };

        var serializedData = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);

        await cache.SetStringAsync(
            cacheKey,
            serializedData,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(4),
            }
        );
        LogHelper.Info($"Successfully added new entry for key: {cacheKey}", log);
    }

    public async Task<string> GetNewTeamMemberFromCacheAsync(Guid memberId)
    {
        var cacheKey = GetKey(memberId.ToString());

        var cachedData = await cache.GetStringAsync(cacheKey);
        if (string.IsNullOrEmpty(cachedData))
        {
            LogHelper.Error($"No cache data found for key: {cacheKey}", log);
            return string.Empty;
        }

        var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(cachedData);
        if (dict == null || !dict.TryGetValue("Team Name", out var teamNameObj))
        {
            LogHelper.Error(
                $"Dictionnary is null or key 'Team Name' missing in cache data: {cacheKey}",
                log
            );
            throw new InvalidOperationException($"Dictionnary is null or key 'Team Name' missing");
        }
        var teamName =
            teamNameObj?.ToString() ?? throw new InvalidOperationException("'Team Name' is null");
        await cache.RemoveAsync(cacheKey);
        return teamName;
    }
}
