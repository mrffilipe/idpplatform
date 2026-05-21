namespace IdPPlatform.Application.Services.Oidc;

public interface IOidcTokenService
{
    Task<(OidcTokenResponse? Response, OidcError? Error)> ExchangeAsync(OidcTokenRequest request, CancellationToken cancellationToken = default);
}
