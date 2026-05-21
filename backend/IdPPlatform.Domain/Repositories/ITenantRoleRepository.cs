using IdPPlatform.Domain.Entities;
using IdPPlatform.Domain.ValueObjects;

namespace IdPPlatform.Domain.Repositories;

public interface ITenantRoleRepository
{
    Task AddAsync(TenantRole role, CancellationToken cancellationToken = default);

    Task<TenantRole?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TenantRole>> ListByTenantIdAsync(
        Guid tenantId,
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TenantRole>> ListByTenantIdAndKeysAsync(
        Guid tenantId,
        IReadOnlyCollection<TenantRoleKey> keys,
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    Task<bool> KeyAlreadyExistsAsync(Guid tenantId, TenantRoleKey key, CancellationToken cancellationToken = default);
}
