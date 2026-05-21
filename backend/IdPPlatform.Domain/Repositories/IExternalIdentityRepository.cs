using IdPPlatform.Domain.Entities;

namespace IdPPlatform.Domain.Repositories;

public interface IExternalIdentityRepository
{
    Task AddAsync(ExternalIdentity externalIdentity, CancellationToken cancellationToken = default);

    Task<ExternalIdentity?> GetByProviderAndProviderUserIdAsync(
        string provider,
        string providerUserId,
        CancellationToken cancellationToken = default);
}
