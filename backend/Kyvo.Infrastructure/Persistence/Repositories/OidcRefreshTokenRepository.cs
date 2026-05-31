using Kyvo.Domain.Entities;
using Kyvo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Kyvo.Infrastructure.Persistence.Repositories;

public sealed class OidcRefreshTokenRepository : IOidcRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;

    public OidcRefreshTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(OidcRefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        return _context.OidcRefreshTokens
            .AddAsync(refreshToken, cancellationToken)
            .AsTask();
    }

    public Task<OidcRefreshToken?> GetByTokenHashForUpdateAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return _context.OidcRefreshTokens
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);
    }
}
