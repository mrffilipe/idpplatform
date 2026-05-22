using IdPPlatform.Domain.Entities;
using IdPPlatform.Domain.Enums;
using IdPPlatform.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IdPPlatform.Infrastructure.Persistence.Repositories;

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
