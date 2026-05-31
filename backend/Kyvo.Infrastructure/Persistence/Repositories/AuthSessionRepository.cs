using Kyvo.Domain.Entities;
using Kyvo.Domain.Enums;
using Kyvo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Kyvo.Infrastructure.Persistence.Repositories;

public sealed class AuthSessionRepository : IAuthSessionRepository
{
    private readonly ApplicationDbContext _context;

    public AuthSessionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(AuthSession session, CancellationToken cancellationToken = default)
    {
        return _context.AuthSessions
            .AddAsync(session, cancellationToken)
            .AsTask();
    }

    public Task<AuthSession?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.AuthSessions
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<AuthSession>> ListActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.AuthSessions
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Status == SessionStatus.Active)
            .OrderByDescending(x => x.LastActivityAt)
            .ToListAsync(cancellationToken);
    }
}
