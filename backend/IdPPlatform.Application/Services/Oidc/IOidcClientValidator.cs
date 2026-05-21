using IdPPlatform.Domain.Entities;

namespace IdPPlatform.Application.Services.Oidc;

public interface IOidcClientValidator
{
    Task<(ApplicationClient? Client, OidcError? Error)> ValidateClientAsync(
        string? clientId,
        string? clientSecret,
        CancellationToken cancellationToken = default);

    OidcError? ValidateRedirectUri(ApplicationClient client, string? redirectUri);

    OidcError? ValidateScopes(ApplicationClient client, IReadOnlyList<string> requestedScopes);

    OidcError? ValidatePkceForAuthorize(
        ApplicationClient client,
        string? codeChallenge,
        string? codeChallengeMethod);

    OidcError? ValidatePkceForToken(
        string codeChallenge,
        string codeChallengeMethod,
        string? codeVerifier);

    IReadOnlyList<string> ParseScopes(string? scope);
}
