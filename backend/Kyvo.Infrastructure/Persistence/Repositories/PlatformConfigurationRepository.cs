using Kyvo.Domain.Entities;
using Kyvo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Kyvo.Infrastructure.Persistence.Repositories;

public sealed class PlatformConfigurationRepository : IPlatformConfigurationRepository
{
    private readonly ApplicationDbContext _context;

    public PlatformConfigurationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<PlatformConfiguration?> GetForUpdateAsync(CancellationToken cancellationToken = default)
    {
        return _context.PlatformConfigurations
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task AddAsync(PlatformConfiguration platformConfiguration, CancellationToken cancellationToken = default)
    {
        return _context.PlatformConfigurations
            .AddAsync(platformConfiguration, cancellationToken)
            .AsTask();
    }
}
