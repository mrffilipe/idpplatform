namespace IdPPlatform.Application.Services.Oidc;

public interface IOidcTokenService
{
    Task<(OidcTokenResponse? Response, OidcError? Error)> ExchangeAsync(OidcTokenRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Issues fresh tokens for the current session (e.g., after subscribe, when the access token does not yet contain tid).
    /// </summary>
    Task<(OidcTokenResponse? Response, OidcError? Error)> IssueForSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
}
