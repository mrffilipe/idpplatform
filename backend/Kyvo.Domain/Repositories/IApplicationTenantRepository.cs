using Kyvo.Domain.Entities;

namespace Kyvo.Domain.Repositories;

public interface IApplicationTenantRepository
{
    Task AddAsync(ApplicationTenant applicationTenant, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        Guid applicationId,
        Guid tenantId,
        CancellationToken cancellationToken = default);
}
