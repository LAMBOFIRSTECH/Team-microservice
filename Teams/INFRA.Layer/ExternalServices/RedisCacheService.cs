using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Hangfire;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Teams.APP.Layer.Interfaces;

namespace Teams.INFRA.Layer.ExternalServices;

public class RedisCacheService : IRedisCacheService
{
    private readonly IDistributedCache _cache;
    private readonly IMemoryCache cacheMemory;
    private readonly ILogger<RedisCacheService> logger;
    private readonly string? cacheKey;

    public RedisCacheService(
        IDistributedCache cache,
        ILogger<RedisCacheService> logger,
        IMemoryCache cacheMemory
    )
    {
        _cache = cache;
        this.logger = logger;
        // cacheKey = $"ExternalData_{GenerateRedisKeyForExternalData()}";
        this.cacheMemory = cacheMemory;
    }

    public void StoreNewTeamMemberInformationsInRedis(Guid memberId, string teamName)
    {
        Dictionary<string, object> jsonObject = new()
        {
            { "Id member", memberId },
            { "Team Name", teamName },
        };
        var cacheKey = $"EntryRedisId-for-{memberId}_{teamName}";
        var cachedData = _cache.GetStringAsync(cacheKey);
        if (cachedData is not null)
        {
            var serializedData = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
            _cache.SetStringAsync(
                cacheKey,
                serializedData,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7),
                }
            );
            logger.LogInformation(
                "Successfull storage refresh token connection for key: {CacheKey}",
                cacheKey
            );
        }
    }

    public void GetNewTeamMemberFromCacheAsync()
    {
        throw new NotImplementedException();
    }
}
