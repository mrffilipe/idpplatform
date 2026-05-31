using Kyvo.Domain.Entities;

namespace Kyvo.Domain.Repositories;

public interface IApplicationRepository
{
    Task AddAsync(Application application, CancellationToken cancellationToken = default);

    Task<Application?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> SlugAlreadyExistsAsync(string slug, CancellationToken cancellationToken = default);
}
