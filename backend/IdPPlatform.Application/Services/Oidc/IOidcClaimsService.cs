using System.Security.Claims;

namespace IdPPlatform.Application.Services.Oidc;

public interface IOidcClaimsService
{
    Task<IReadOnlyList<Claim>?> TryBuildClaimsAsync(
        Guid sessionId,
        IReadOnlyList<string> scopes,
        CancellationToken cancellationToken = default);
}
