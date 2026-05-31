using Kyvo.Domain.Entities;
using Kyvo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Kyvo.Infrastructure.Persistence.Repositories;

public sealed class TenantRepository : ITenantRepository
{
    private readonly ApplicationDbContext _context;

    public TenantRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        return _context.Tenants
            .AddAsync(tenant, cancellationToken)
            .AsTask();
    }

    public Task<Tenant?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Tenants
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<bool> KeyAlreadyExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        var normalized = key.Trim().ToLowerInvariant();
        return _context.Tenants
            .AnyAsync(x => x.Key.Value == normalized, cancellationToken);
    }
}
