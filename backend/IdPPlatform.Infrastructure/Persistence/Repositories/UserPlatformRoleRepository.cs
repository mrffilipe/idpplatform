using IdPPlatform.Domain.Entities;
using IdPPlatform.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IdPPlatform.Infrastructure.Persistence.Repositories;

public sealed class UserPlatformRoleRepository : IUserPlatformRoleRepository
{
    private readonly ApplicationDbContext _context;

    public UserPlatformRoleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(UserPlatformRole assignment, CancellationToken cancellationToken = default)
    {
        return _context.UserPlatformRoles
            .AddAsync(assignment, cancellationToken)
            .AsTask();
    }

    public async Task<IReadOnlyList<UserPlatformRole>> ListByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserPlatformRoles
            .Include(x => x.Role)
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        return _context.UserPlatformRoles
            .AnyAsync(x => x.UserId == userId && x.RoleId == roleId, cancellationToken);
    }
}
