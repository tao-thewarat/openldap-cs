using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using OpenLdapCs.Interfaces;
using OpenLdapCs.Models;

namespace OpenLdapCs.Services;

public sealed class RedisRefreshTokenStore : IRefreshTokenStore
{
    private readonly IDistributedCache distributedCache;

    public RedisRefreshTokenStore(IDistributedCache distributedCache)
    {
        this.distributedCache = distributedCache;
    }

    public async Task StoreAsync(
        string refreshToken,
        RefreshTokenRecord record,
        TimeSpan timeToLive,
        CancellationToken cancellationToken = default
    )
    {
        var payload = JsonSerializer.Serialize(record);
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = timeToLive,
        };

        await distributedCache.SetStringAsync(
            BuildKey(refreshToken),
            payload,
            cacheOptions,
            cancellationToken
        );
    }

    public async Task<RefreshTokenRecord?> GetAsync(
        string refreshToken,
        CancellationToken cancellationToken = default
    )
    {
        var payload = await distributedCache.GetStringAsync(
            BuildKey(refreshToken),
            cancellationToken
        );
        return string.IsNullOrWhiteSpace(payload)
            ? null
            : JsonSerializer.Deserialize<RefreshTokenRecord>(payload);
    }

    public Task RemoveAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return distributedCache.RemoveAsync(BuildKey(refreshToken), cancellationToken);
    }

    private static string BuildKey(string refreshToken)
    {
        return $"refresh-token:{refreshToken}";
    }
}
