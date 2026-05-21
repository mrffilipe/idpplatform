using System.Text.Json;
using IdPPlatform.Infrastructure.Configurations;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TenancyKit.Abstractions;

namespace IdPPlatform.Infrastructure.Persistence;

public sealed class TenantStore : ITenantStore<TenantInfoAdapter>
{
    private readonly ApplicationDbContext _context;
    private readonly IDistributedCache _distributedCache;
    private readonly RedisOptions _redisOptions;

    public TenantStore(
        ApplicationDbContext context,
        IDistributedCache distributedCache,
        IOptions<RedisOptions> redisOptions)
    {
        _context = context;
        _distributedCache = distributedCache;
        _redisOptions = redisOptions.Value;
    }

    public async Task<IReadOnlyList<TenantInfoAdapter>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tenants = await _context.Tenants
            .AsNoTracking()
            .Where(x => x.IsActive)
            .ToListAsync(cancellationToken);
        return tenants.Select(x => new TenantInfoAdapter(x)).ToList();
    }

    public async Task<TenantInfoAdapter?> GetByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
    {
        var normalized = identifier.Trim().ToLowerInvariant();
        var cacheKey = TenantCacheKeys.BuildIdentifierKey(normalized);

        try
        {
            var cached = await _distributedCache.GetStringAsync(cacheKey, cancellationToken);
            if (!string.IsNullOrWhiteSpace(cached))
            {
                var cachedValue = JsonSerializer.Deserialize<CachedTenantInfo>(cached);
                if (cachedValue is not null)
                {
                    return new TenantInfoAdapter(cachedValue.Id, cachedValue.Identifier, cachedValue.Name);
                }
            }
        }
        catch
        {
            // Cache indisponível (ex.: Redis offline) — segue para o banco.
        }

        var tenant = await _context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Key.Value == normalized && x.IsActive, cancellationToken);

        if (tenant is null)
        {
            return null;
        }

        var adapter = new TenantInfoAdapter(tenant);

        try
        {
            var entry = new CachedTenantInfo(adapter.Id, adapter.Identifier, adapter.Name);
            await _distributedCache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(entry),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow =
                        TimeSpan.FromMinutes(Math.Max(1, _redisOptions.TenantIdentifierCacheMinutes))
                },
                cancellationToken);
        }
        catch
        {
            // Ignora falha de escrita no cache.
        }

        return adapter;
    }

    private sealed record CachedTenantInfo(string Id, string Identifier, string Name);
}
