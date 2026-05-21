namespace IdPPlatform.Application.Services.Oidc;

public interface IOidcTokenService
{
    Task<(OidcTokenResponse? Response, OidcError? Error)> ExchangeAsync(OidcTokenRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Emite novos tokens para a sessão atual (ex.: após subscribe, quando o access token ainda não tem tid).
    /// </summary>
    Task<(OidcTokenResponse? Response, OidcError? Error)> IssueForSessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);
}
