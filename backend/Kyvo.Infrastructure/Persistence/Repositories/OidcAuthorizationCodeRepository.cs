using Kyvo.Domain.Entities;
using Kyvo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Kyvo.Infrastructure.Persistence.Repositories;

public sealed class OidcAuthorizationCodeRepository : IOidcAuthorizationCodeRepository
{
    private readonly ApplicationDbContext _context;

    public OidcAuthorizationCodeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(OidcAuthorizationCode authorizationCode, CancellationToken cancellationToken = default)
    {
        return _context.OidcAuthorizationCodes
            .AddAsync(authorizationCode, cancellationToken)
            .AsTask();
    }

    public Task<OidcAuthorizationCode?> GetByCodeHashForUpdateAsync(
        string codeHash,
        CancellationToken cancellationToken = default)
    {
        return _context.OidcAuthorizationCodes
            .FirstOrDefaultAsync(x => x.CodeHash == codeHash, cancellationToken);
    }
}
