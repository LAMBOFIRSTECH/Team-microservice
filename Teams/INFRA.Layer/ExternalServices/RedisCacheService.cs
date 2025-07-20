using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Teams.APP.Layer.Interfaces;

namespace Teams.INFRA.Layer.ExternalServices;

public class RedisCacheService(
    IDistributedCache cache,
    ILogger<RedisCacheService> logger
// IMemoryCache cacheMemory
) : IRedisCacheService
{
    private string GetKey(string key) => $"DevCache:{key}";

    // HGET DevCache:12345678-90ab-cdef-1234-567890abcdef data

    public async Task StoreNewTeamMemberInformationsInRedisAsync(Guid memberId, string teamName)
    {
        var cacheKey = GetKey(memberId.ToString());

        var cachedData = await cache.GetStringAsync(cacheKey);

        if (cachedData is not null)
        {
            logger.LogError("Key already exists in Redis: {CacheKey}", cacheKey);
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
        logger.LogInformation("Successfully added new entry for key: {CacheKey}", cacheKey);
    }

    public async Task<string> GetNewTeamMemberFromCacheAsync(Guid memberId)
    {
        var cacheKey = GetKey(memberId.ToString());

        var cachedData = await cache.GetStringAsync(cacheKey);
        if (string.IsNullOrEmpty(cachedData))
        {
            logger.LogWarning("💢 No cache data found for key: {CacheKey}", cacheKey);
            throw new KeyNotFoundException($"Cache key not found: {cacheKey}");
        }

        var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(cachedData);
        if (dict == null || !dict.TryGetValue("Team Name", out var teamNameObj))
            throw new InvalidOperationException($"Dictionnary is null or key 'Team Name' missing");
        var teamName =
            teamNameObj?.ToString() ?? throw new InvalidOperationException("'Team Name' is null");
        await cache.RemoveAsync(cacheKey);
        return teamName;
    }
}
