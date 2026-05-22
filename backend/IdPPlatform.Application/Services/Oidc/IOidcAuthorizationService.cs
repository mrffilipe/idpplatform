namespace IdPPlatform.Application.Services.Oidc;

public interface IOidcAuthorizationService
{
    OidcError? ValidateAuthorizeRequest(OidcAuthorizeRequest request, ApplicationClientValidationContext clientContext);

    Task<(string? Code, OidcError? Error)> CreateAuthorizationCodeAsync(
        OidcAuthorizeRequest request,
        Guid authSessionId,
        Guid applicationClientId,
        IReadOnlyList<string> scopes,
        CancellationToken cancellationToken = default);
}
