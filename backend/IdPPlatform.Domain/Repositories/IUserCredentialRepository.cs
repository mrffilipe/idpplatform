using IdPPlatform.Domain.Entities;

namespace IdPPlatform.Domain.Repositories;

public interface IUserCredentialRepository
{
    Task AddAsync(UserCredential credential, CancellationToken cancellationToken = default);

    Task<UserCredential?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
