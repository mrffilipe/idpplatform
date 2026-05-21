using IdPPlatform.Application.Services.TenantResolutionCache;
using IdPPlatform.Infrastructure.Persistence;
using Microsoft.Extensions.Caching.Distributed;

namespace IdPPlatform.Infrastructure.Services.TenantResolutionCache;

public sealed class DistributedTenantResolutionCache : ITenantResolutionCache
{
    private readonly IDistributedCache _distributedCache;

    public DistributedTenantResolutionCache(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public Task InvalidateByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
    {
        var cacheKey = TenantCacheKeys.BuildIdentifierKey(identifier);
        return _distributedCache.RemoveAsync(cacheKey, cancellationToken);
    }
}
