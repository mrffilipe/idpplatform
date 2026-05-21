using IdPPlatform.Domain.Entities;

namespace IdPPlatform.Domain.Repositories;

public interface ITenantRepository
{
    Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default);

    Task<Tenant?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> KeyAlreadyExistsAsync(string key, CancellationToken cancellationToken = default);
}
