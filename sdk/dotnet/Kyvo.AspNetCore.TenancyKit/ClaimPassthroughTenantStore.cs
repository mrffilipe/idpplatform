using TenancyKit.Abstractions;

namespace Kyvo.AspNetCore.TenancyKit;

/// <summary>
/// Resolves tenant by Guid identifier from JWT without calling Kyvo (v1).
/// </summary>
public sealed class ClaimPassthroughTenantStore<TTenantInfo> : ITenantStore<TTenantInfo>
    where TTenantInfo : ProductTenantInfo, new()
{
    public Task<TTenantInfo?> GetByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(identifier, out _))
        {
            return Task.FromResult<TTenantInfo?>(null);
        }

        var id = identifier;
        var info = new TTenantInfo
        {
            Id = id,
            Identifier = id
        };

        return Task.FromResult<TTenantInfo?>(info);
    }
}
