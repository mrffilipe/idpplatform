using IdPPlatform.Application.Exceptions;
using IdPPlatform.Application.Services.ExternalIdentityProvider;
using IdPPlatform.Application.Services.IdentityProvider;
using IdPPlatform.Domain.Exceptions;

namespace IdPPlatform.Infrastructure.Services.ExternalIdentityProvider;

public sealed class CognitoTokenValidator : IIdentityProviderTokenValidator
{
    public Task<ExternalAuthResult> ValidateAsync(
        Domain.Entities.IdentityProvider provider,
        string identityToken,
        CancellationToken cancellationToken = default)
    {
        throw new DomainBusinessRuleException(ApplicationErrorMessages.IdentityProvider.LoginTypeNotSupported);
    }
}
