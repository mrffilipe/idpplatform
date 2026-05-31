using Kyvo.Domain.Entities;
using Kyvo.Domain.Enums;

namespace Kyvo.Domain.Repositories;

public interface IIdentityProviderRepository
{
    Task AddAsync(IdentityProvider provider, CancellationToken cancellationToken = default);

    Task<IdentityProvider?> GetByAliasAsync(string alias, CancellationToken cancellationToken = default);

    Task<IdentityProvider?> GetEnabledByAliasAsync(string alias, CancellationToken cancellationToken = default);

    Task<IdentityProvider?> GetEnabledByTypeAsync(IdentityProviderType type, CancellationToken cancellationToken = default);

    Task<IdentityProvider?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IdentityProvider>> ListEnabledAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IdentityProvider>> ListEnabledByCapabilityAsync(IdpCapability capability, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IdentityProvider>> ListAllAsync(CancellationToken cancellationToken = default);

    Task<bool> AliasAlreadyExistsAsync(string alias, CancellationToken cancellationToken = default);

    Task<bool> AnyEnabledLocalProviderAsync(CancellationToken cancellationToken = default);
}
