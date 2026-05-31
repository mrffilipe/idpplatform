using System.Security.Claims;

namespace Kyvo.Application.Services.Oidc;

public interface IOidcClaimsService
{
    Task<IReadOnlyList<Claim>?> TryBuildClaimsAsync(
        Guid sessionId,
        IReadOnlyList<string> scopes,
        CancellationToken cancellationToken = default);
}
