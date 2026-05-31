using Kyvo.Domain.Entities;

namespace Kyvo.Domain.Repositories;

public interface ITenantMembershipRepository
{
    Task AddAsync(TenantMembership membership, CancellationToken cancellationToken = default);

    Task<TenantMembership?> GetForUpdateWithRolesAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TenantMembership?> GetByUserIdAndTenantIdWithRolesAsync(
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TenantMembership>> ListByUserIdWithTenantAndRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
