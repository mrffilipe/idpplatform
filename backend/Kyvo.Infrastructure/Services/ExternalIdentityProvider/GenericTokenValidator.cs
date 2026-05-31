using Kyvo.Application.Exceptions;
using Kyvo.Application.Services.ExternalIdentityProvider;
using Kyvo.Application.Services.IdentityProvider;
using Kyvo.Domain.Exceptions;

namespace Kyvo.Infrastructure.Services.ExternalIdentityProvider;

public sealed class GenericTokenValidator : IIdentityProviderTokenValidator
{
    public Task<ExternalAuthResult> ValidateAsync(
        Domain.Entities.IdentityProvider provider,
        string identityToken,
        CancellationToken cancellationToken = default)
    {
        throw new DomainBusinessRuleException(ApplicationErrorMessages.IdentityProvider.LoginTypeNotSupported);
    }
}
