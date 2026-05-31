using Kyvo.Domain.Entities;

namespace Kyvo.Domain.Repositories;

public interface IPlatformConfigurationRepository
{
    Task AddAsync(PlatformConfiguration platformConfiguration, CancellationToken cancellationToken = default);

    Task<PlatformConfiguration?> GetForUpdateAsync(CancellationToken cancellationToken = default);
}
