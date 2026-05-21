using IdPPlatform.Domain.Entities;

namespace IdPPlatform.Domain.Repositories;

public interface IApplicationRepository
{
    Task AddAsync(Application application, CancellationToken cancellationToken = default);

    Task<Application?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> SlugAlreadyExistsAsync(string slug, CancellationToken cancellationToken = default);
}
