using IdPPlatform.Domain.Entities;

namespace IdPPlatform.Domain.Repositories;

public interface IOidcAuthorizationCodeRepository
{
    Task AddAsync(OidcAuthorizationCode authorizationCode, CancellationToken cancellationToken = default);

    Task<OidcAuthorizationCode?> GetByCodeHashForUpdateAsync(string codeHash, CancellationToken cancellationToken = default);
}
