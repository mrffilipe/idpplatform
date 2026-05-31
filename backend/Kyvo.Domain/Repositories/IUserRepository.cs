using Kyvo.Domain.Entities;

namespace Kyvo.Domain.Repositories;

public interface IUserRepository
{
    Task AddAsync(User user, CancellationToken cancellationToken = default);

    Task<User?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> EmailAlreadyExistsAsync(string email, CancellationToken cancellationToken = default);
}
