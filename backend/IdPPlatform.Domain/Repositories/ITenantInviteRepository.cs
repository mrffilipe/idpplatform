using IdPPlatform.Domain.Entities;

namespace IdPPlatform.Domain.Repositories;

public interface ITenantInviteRepository
{
    Task AddAsync(TenantInvite invite, CancellationToken cancellationToken = default);

    Task<TenantInvite?> GetByTokenHashWithRolesAsync(string tokenHash, CancellationToken cancellationToken = default);
}
