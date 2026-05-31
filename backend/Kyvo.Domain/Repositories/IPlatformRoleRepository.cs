using Kyvo.Domain.Entities;

namespace Kyvo.Domain.Repositories;

public interface IPlatformRoleRepository
{
    Task AddAsync(PlatformRole role, CancellationToken cancellationToken = default);

    Task<PlatformRole?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PlatformRole>> ListAllAsync(CancellationToken cancellationToken = default);

    Task<bool> KeyAlreadyExistsAsync(string key, CancellationToken cancellationToken = default);
}
