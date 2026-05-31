using Kyvo.Domain.Entities;

namespace Kyvo.Domain.Repositories;

public interface IOidcAuthorizationCodeRepository
{
    Task AddAsync(OidcAuthorizationCode authorizationCode, CancellationToken cancellationToken = default);

    Task<OidcAuthorizationCode?> GetByCodeHashForUpdateAsync(string codeHash, CancellationToken cancellationToken = default);
}
