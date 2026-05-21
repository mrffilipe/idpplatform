namespace IdPPlatform.Application.Services.ExternalIdentityProvider;

public interface IExternalIdentityProvider
{
    Task<ExternalAuthResult> ValidateAsync(string identityToken, CancellationToken cancellationToken = default);
}
