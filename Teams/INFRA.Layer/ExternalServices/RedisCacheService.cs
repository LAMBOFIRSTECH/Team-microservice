using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Teams.APP.Layer.Helpers;
using Teams.APP.Layer.Interfaces;
using Teams.CORE.Layer.Entities;
using Teams.CORE.Layer.Interfaces;

namespace Teams.INFRA.Layer.ExternalServices;
public class RedisCacheService(
    IDistributedCache cache,
    ITeamRepository teamRepository,
    ILogger<RedisCacheService> log
) : IRedisCacheService
{
    private string GetKey(string key) => $"DevCache:{key}";

    // HGET DevCache:12345678-90ab-cdef-1234-567890abcdef data

    public async Task StoreArchivedTeamInRedisAsync(
        Team team,
        CancellationToken cancellationToken = default
    )
    {
        var cacheKey = $"DevCache:{team.Id}"; // a terme mettre le nom de l'équipe

        try
        {
            var cachedData = await cache.GetStringAsync(cacheKey, cancellationToken);
            if (cachedData is not null)
            {
                LogHelper.Warning($"Key already exists in Redis: {cacheKey}", log);
                throw new InvalidOperationException();
            }

            var serializedTeam = JsonConvert.SerializeObject(team, Formatting.None);
            await cache.SetStringAsync(
                cacheKey,
                serializedTeam,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30),
                },
                cancellationToken
            );

            LogHelper.Info($"✅ Team {team.Name} stored in Redis with key {cacheKey}", log);
            await teamRepository.DeleteTeamAsync(team.Id, cancellationToken);
            LogHelper.Info($"✅ Team {team.Name} has been delete from DataBase successfully.", log);
        }
        catch (Exception ex)
        {
            LogHelper.Error($"❌ Failed to store team {team.Name} in Redis: {ex.Message}", log);
            throw;
        }
    }
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
