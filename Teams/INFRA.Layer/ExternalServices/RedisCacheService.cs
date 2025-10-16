using Teams.API.Layer.DTOs;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Teams.APP.Layer.Helpers;
using Teams.APP.Layer.Interfaces;
using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.INFRA.Layer.Exceptions;

namespace Teams.INFRA.Layer.ExternalServices;

public class RedisCacheService(
    IDistributedCache cache,
    ITeamRepository teamRepository,
    ILogger<RedisCacheService> log
) : IRedisCacheService
{
    private static string BuildKey(string key) => $"DevCache:{key}";

    private async Task EnsureKeyDoesNotExistAsync(string cacheKey, CancellationToken cancellationToken)
    {
        var cachedData = await cache.GetStringAsync(cacheKey, cancellationToken);
        if (cachedData is not null)
        {
            LogHelper.Warning($"Key already exists in Redis: {cacheKey}", log);
            throw InfrastructureException.InfraError(
                title: "Conflict key",
                statusCode: 409,
                message: $"Key already exists in Redis: {cacheKey}",
                reason: "Infrastructure validation error"
            );
        }
    }

    private async Task<string> EnsureKeyExistsAsync(string cacheKey, CancellationToken cancellationToken)
    {
        var cachedData = await cache.GetStringAsync(cacheKey, cancellationToken);
        if (string.IsNullOrEmpty(cachedData))
        {
            LogHelper.Error($"Key not found in Redis: {cacheKey}", log);
            return $"Resource with key {cacheKey} not found in redis.";
        }
        return cachedData;
    }
    public async Task StoreArchivedTeamInRedisAsync(TeamDetailsDto redisTeamDto, CancellationToken cancellationToken)
    {
        var cacheKey = BuildKey(redisTeamDto.Id.ToString());
        await EnsureKeyDoesNotExistAsync(cacheKey, cancellationToken);
        try
        {
            var serializedTeam = JsonConvert.SerializeObject(redisTeamDto, Formatting.Indented);
            await cache.SetStringAsync(
                cacheKey,
                serializedTeam,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) // 1 semaine en prod
                },
                cancellationToken
            );

            LogHelper.Info($"✅ Team {redisTeamDto.Name} stored in Redis with key {cacheKey}", log);
            await teamRepository.DeleteTeamAsync(redisTeamDto.Id, cancellationToken);
            LogHelper.Info($"✅ Team {redisTeamDto.Name} has been deleted from DB successfully.", log);
        }
        catch (Exception ex)
        {
            LogHelper.Error($"❌ Failed to store team {redisTeamDto.Name} in Redis: {ex.Message}", log);
            throw;
        }
    }

    /// <summary>
    /// Retrieves an archived team from Redis cache using the team's ID.
    /// If the data is found in the cache, it is deserialized into a `Team` object.
    /// </summary>
    /// <param name="teamId">The unique identifier of the team to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token to handle task cancellation.</param>
    /// <returns>A `Team` object representing the archived team retrieved from Redis.</returns>
    /// <exception cref="InvalidOperationException">Thrown if deserialization fails or if the resulting team object is null.</exception>
    public async Task<TeamDetailsDto> GetArchivedTeamFromRedisAsync(Guid teamId, CancellationToken cancellationToken)
    {
        var cacheKey = BuildKey(teamId.ToString());
        var cachedData = await EnsureKeyExistsAsync(cacheKey, cancellationToken);
        if (cachedData.StartsWith("Resource with key")) return new TeamDetailsDto();
        var teamDto = JsonConvert.DeserializeObject<TeamDetailsDto>(cachedData) ?? throw new InvalidOperationException("Deserialized team is null");
        return teamDto;
    }

    public async Task StoreNewTeamMemberInformationsInRedisAsync(Guid memberId, string teamName, CancellationToken cancellationToken)
    {
        var cacheKey = BuildKey(memberId.ToString());
        await EnsureKeyDoesNotExistAsync(cacheKey, cancellationToken);

        var jsonObject = new Dictionary<string, object>
        {
            { "Id member", memberId },
            { "Team Name", teamName }
        };

        var serializedData = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
        await cache.SetStringAsync(
            cacheKey,
            serializedData,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) // 1 semaine en prod
            },
            cancellationToken
        );
        LogHelper.Info($"✅ Successfully added new entry for key: {cacheKey}", log);
    }
    public async Task<string> GetNewTeamMemberFromCacheAsync(Guid memberId, CancellationToken cancellationToken)
    {
        var cacheKey = BuildKey(memberId.ToString());
        var cachedData = await EnsureKeyExistsAsync(cacheKey, cancellationToken);
        if (cachedData.StartsWith("Resource with key")) return string.Empty;
        var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(cachedData);
        if (dict == null || !dict.TryGetValue("Team Name", out var teamNameObj))
        {
            LogHelper.Error($"Dictionary is null or key 'Team Name' missing in cache data for {cacheKey}", log);
            throw new InvalidOperationException("Dictionary is null or key 'Team Name' missing");
        }
        var teamName = teamNameObj?.ToString() ?? throw new InvalidOperationException("'Team Name' is null");
        await cache.RemoveAsync(cacheKey, cancellationToken);
        return teamName;
    }
}
