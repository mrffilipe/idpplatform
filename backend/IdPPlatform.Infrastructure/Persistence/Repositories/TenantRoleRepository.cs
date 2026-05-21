using IdPPlatform.Domain.Entities;
using IdPPlatform.Domain.Repositories;
using IdPPlatform.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace IdPPlatform.Infrastructure.Persistence.Repositories;

public sealed class TenantRoleRepository : ITenantRoleRepository
{
    private readonly ApplicationDbContext _context;

    public TenantRoleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(TenantRole role, CancellationToken cancellationToken = default)
    {
        return _context.TenantRoles
            .AddAsync(role, cancellationToken)
            .AsTask();
    }

    public Task<TenantRole?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.TenantRoles
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<TenantRole>> ListByTenantIdAsync(
        Guid tenantId,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.TenantRoles
            .Where(x => x.TenantId == tenantId);
        if (!includeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        return await query
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TenantRole>> ListByTenantIdAndKeysAsync(
        Guid tenantId,
        IReadOnlyCollection<TenantRoleKey> keys,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var values = keys.Select(x => x.Value).ToList();
        var query = _context.TenantRoles
            .Where(x => x.TenantId == tenantId && values.Contains(x.Key.Value));

        if (activeOnly)
        {
            query = query.Where(x => x.IsActive);
        }

        return await query
            .ToListAsync(cancellationToken);
    }

    public Task<bool> KeyAlreadyExistsAsync(
        Guid tenantId,
        TenantRoleKey key,
        CancellationToken cancellationToken = default)
    {
        return _context.TenantRoles
            .AnyAsync(x => x.TenantId == tenantId && x.Key.Value == key.Value, cancellationToken);
    }
}
