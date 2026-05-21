using IdPPlatform.Domain.Entities;
using IdPPlatform.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IdPPlatform.Infrastructure.Persistence.Repositories;

public sealed class TenantMembershipRepository : ITenantMembershipRepository
{
    private readonly ApplicationDbContext _context;

    public TenantMembershipRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(TenantMembership membership, CancellationToken cancellationToken = default)
    {
        return _context.TenantMemberships
            .AddAsync(membership, cancellationToken)
            .AsTask();
    }

    public Task<TenantMembership?> GetForUpdateWithRolesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.TenantMemberships
            .Include(x => x.Roles)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<TenantMembership?> GetByUserIdAndTenantIdWithRolesAsync(
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return _context.TenantMemberships
            .Include(x => x.Roles)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.TenantId == tenantId, cancellationToken);
    }

    public async Task<IReadOnlyList<TenantMembership>> ListByUserIdWithTenantAndRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.TenantMemberships
            .AsNoTracking()
            .Include(x => x.Tenant)
            .Include(x => x.Roles)
            .ThenInclude(x => x.Role)
            .Where(x => x.UserId == userId && x.IsActive)
            .ToListAsync(cancellationToken);
    }
}
