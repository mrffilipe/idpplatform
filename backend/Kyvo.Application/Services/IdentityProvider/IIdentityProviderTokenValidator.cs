using Kyvo.Application.Services.ExternalIdentityProvider;

namespace Kyvo.Application.Services.IdentityProvider;

public interface IIdentityProviderTokenValidator
{
    Task<ExternalAuthResult> ValidateAsync(
        Domain.Entities.IdentityProvider provider,
        string identityToken,
        CancellationToken cancellationToken = default);
}
