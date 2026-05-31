using Kyvo.Domain.Entities;
using Kyvo.Domain.Enums;
using Kyvo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Kyvo.Infrastructure.Persistence.Repositories;

public sealed class IdentityProviderRepository : IIdentityProviderRepository
{
    private readonly ApplicationDbContext _context;

    public IdentityProviderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(IdentityProvider provider, CancellationToken cancellationToken = default)
    {
        return _context.IdentityProviders
            .AddAsync(provider, cancellationToken)
            .AsTask();
    }

    public Task<IdentityProvider?> GetByAliasAsync(string alias, CancellationToken cancellationToken = default)
    {
        var normalized = alias.Trim().ToLowerInvariant();
        return _context.IdentityProviders
            .FirstOrDefaultAsync(x => x.Alias == normalized, cancellationToken);
    }

    public Task<IdentityProvider?> GetEnabledByAliasAsync(string alias, CancellationToken cancellationToken = default)
    {
        var normalized = alias.Trim().ToLowerInvariant();
        return _context.IdentityProviders
            .FirstOrDefaultAsync(x => x.Alias == normalized && x.Enabled, cancellationToken);
    }

    public Task<IdentityProvider?> GetEnabledByTypeAsync(IdentityProviderType type, CancellationToken cancellationToken = default)
    {
        return _context.IdentityProviders
            .FirstOrDefaultAsync(x => x.ProviderType == type && x.Enabled, cancellationToken);
    }

    public Task<IdentityProvider?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.IdentityProviders
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<IdentityProvider>> ListEnabledAsync(CancellationToken cancellationToken = default)
    {
        return await _context.IdentityProviders
            .Where(x => x.Enabled)
            .OrderBy(x => x.Alias)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<IdentityProvider>> ListEnabledByCapabilityAsync(
        IdpCapability capability,
        CancellationToken cancellationToken = default)
    {
        // Filtering happens in memory after loading enabled providers because the value-converted
        // collection (IReadOnlyCollection<IdpCapability>) does not translate to SQL reliably across
        // EF Core providers. The set of enabled providers is small (typically <10), so this is fine.
        var enabled = await _context.IdentityProviders
            .Where(x => x.Enabled)
            .OrderBy(x => x.Alias)
            .ToListAsync(cancellationToken);

        return enabled.Where(x => x.Capabilities.Contains(capability)).ToList();
    }

    public async Task<IReadOnlyList<IdentityProvider>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.IdentityProviders
            .OrderBy(x => x.Alias)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> AliasAlreadyExistsAsync(string alias, CancellationToken cancellationToken = default)
    {
        var normalized = alias.Trim().ToLowerInvariant();
        return _context.IdentityProviders
            .AnyAsync(x => x.Alias == normalized, cancellationToken);
    }

    public Task<bool> AnyEnabledLocalProviderAsync(CancellationToken cancellationToken = default)
    {
        return _context.IdentityProviders
            .AnyAsync(x => x.ProviderType == IdentityProviderType.Local && x.Enabled, cancellationToken);
    }
}
