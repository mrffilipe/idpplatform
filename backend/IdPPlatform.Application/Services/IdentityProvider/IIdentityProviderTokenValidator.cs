using IdPPlatform.Application.Services.ExternalIdentityProvider;

namespace IdPPlatform.Application.Services.IdentityProvider;

public interface IIdentityProviderTokenValidator
{
    Task<ExternalAuthResult> ValidateAsync(
        Domain.Entities.IdentityProvider provider,
        string identityToken,
        CancellationToken cancellationToken = default);
}
