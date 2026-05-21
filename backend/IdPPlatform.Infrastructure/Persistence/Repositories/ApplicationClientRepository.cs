using IdPPlatform.Domain.Entities;
using IdPPlatform.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IdPPlatform.Infrastructure.Persistence.Repositories;

public sealed class ApplicationClientRepository : IApplicationClientRepository
{
    private readonly ApplicationDbContext _context;

    public ApplicationClientRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(ApplicationClient client, CancellationToken cancellationToken = default)
    {
        return _context.ApplicationClients
            .AddAsync(client, cancellationToken)
            .AsTask();
    }

    public Task<ApplicationClient?> GetByClientIdAsync(string clientId, CancellationToken cancellationToken = default)
    {
        return _context.ApplicationClients
            .FirstOrDefaultAsync(x => x.ClientId == clientId, cancellationToken);
    }

    public Task<ApplicationClient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.ApplicationClients
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ApplicationClient>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ApplicationClients
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
