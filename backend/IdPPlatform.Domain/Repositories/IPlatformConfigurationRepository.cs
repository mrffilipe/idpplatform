using IdPPlatform.Domain.Entities;

namespace IdPPlatform.Domain.Repositories;

public interface IPlatformConfigurationRepository
{
    Task AddAsync(PlatformConfiguration platformConfiguration, CancellationToken cancellationToken = default);

    Task<PlatformConfiguration?> GetForUpdateAsync(CancellationToken cancellationToken = default);
}
