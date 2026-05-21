using IdPPlatform.Domain.Entities;

namespace IdPPlatform.Domain.Repositories;

public interface IAuthSessionRepository
{
    Task AddAsync(AuthSession session, CancellationToken cancellationToken = default);

    Task<AuthSession?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuthSession>> ListActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
