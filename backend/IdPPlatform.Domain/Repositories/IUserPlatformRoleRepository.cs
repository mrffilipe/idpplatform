using IdPPlatform.Domain.Entities;

namespace IdPPlatform.Domain.Repositories;

public interface IUserPlatformRoleRepository
{
    Task AddAsync(UserPlatformRole assignment, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserPlatformRole>> ListByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken = default);
}
