using Kyvo.Domain.Entities;

namespace Kyvo.Domain.Repositories;

public interface IApplicationClientRepository
{
    Task AddAsync(ApplicationClient client, CancellationToken cancellationToken = default);

    Task<ApplicationClient?> GetByClientIdAsync(string clientId, CancellationToken cancellationToken = default);

    Task<ApplicationClient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ApplicationClient>> ListAllAsync(CancellationToken cancellationToken = default);
}
