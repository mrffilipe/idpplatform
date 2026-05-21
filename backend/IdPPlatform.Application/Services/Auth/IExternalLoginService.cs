namespace IdPPlatform.Application.Services.Auth;

public interface IExternalLoginService
{
    Task<ExternalLoginResult> LoginWithIdentityTokenAsync(string identityToken, CancellationToken cancellationToken = default);
}
