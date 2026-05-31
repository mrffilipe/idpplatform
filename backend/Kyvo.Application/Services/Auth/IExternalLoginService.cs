namespace Kyvo.Application.Services.Auth;

public interface IExternalLoginService
{
    Task<ExternalLoginResult> LoginWithProviderAsync(
        string providerAlias,
        string identityToken,
        CancellationToken cancellationToken = default);
}
