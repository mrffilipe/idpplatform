using Kyvo.Domain.Entities;

namespace Kyvo.Domain.Repositories;

public interface IOidcRefreshTokenRepository
{
    Task AddAsync(OidcRefreshToken refreshToken, CancellationToken cancellationToken = default);

    Task<OidcRefreshToken?> GetByTokenHashForUpdateAsync(string tokenHash, CancellationToken cancellationToken = default);
}
