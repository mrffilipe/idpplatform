using IdPPlatform.Domain.Entities;
using IdPPlatform.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IdPPlatform.Infrastructure.Persistence.Repositories;

public sealed class ExternalIdentityRepository : IExternalIdentityRepository
{
    private readonly ApplicationDbContext _context;

    public ExternalIdentityRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(ExternalIdentity externalIdentity, CancellationToken cancellationToken = default)
    {
        return _context.ExternalIdentities
            .AddAsync(externalIdentity, cancellationToken)
            .AsTask();
    }

    public Task<ExternalIdentity?> GetByProviderAndProviderUserIdAsync(
        string provider,
        string providerUserId,
        CancellationToken cancellationToken = default)
    {
        var normalizedProvider = provider.Trim().ToLowerInvariant();
        return _context.ExternalIdentities
            .FirstOrDefaultAsync(
                x => x.Provider == normalizedProvider && x.ProviderUserId == providerUserId,
                cancellationToken);
    }
}
