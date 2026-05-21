using IdPPlatform.Domain.Entities;
using IdPPlatform.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IdPPlatform.Infrastructure.Persistence.Repositories;

public sealed class TenantInviteRepository : ITenantInviteRepository
{
    private readonly ApplicationDbContext _context;

    public TenantInviteRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(TenantInvite invite, CancellationToken cancellationToken = default)
    {
        return _context.Set<TenantInvite>()
            .AddAsync(invite, cancellationToken)
            .AsTask();
    }

    public Task<TenantInvite?> GetByTokenHashWithRolesAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return _context.Set<TenantInvite>()
            .Include(x => x.Roles)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);
    }
}
