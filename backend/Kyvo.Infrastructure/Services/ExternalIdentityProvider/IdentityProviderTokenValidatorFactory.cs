using Kyvo.Application.Exceptions;
using Kyvo.Application.Services.IdentityProvider;
using Kyvo.Domain.Enums;
using Kyvo.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Kyvo.Infrastructure.Services.ExternalIdentityProvider;

public sealed class IdentityProviderTokenValidatorFactory : IIdentityProviderTokenValidatorFactory
{
    private readonly IServiceProvider _serviceProvider;

    public IdentityProviderTokenValidatorFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IIdentityProviderTokenValidator GetValidator(IdentityProviderType providerType)
    {
        return providerType switch
        {
            IdentityProviderType.Firebase => _serviceProvider.GetRequiredService<FirebaseTokenValidator>(),
            IdentityProviderType.Cognito => _serviceProvider.GetRequiredService<CognitoTokenValidator>(),
            IdentityProviderType.Generic => _serviceProvider.GetRequiredService<GenericTokenValidator>(),
            IdentityProviderType.Local => throw new DomainBusinessRuleException(
                ApplicationErrorMessages.IdentityProvider.LocalNotAllowedForExternalLogin),
            _ => throw new DomainBusinessRuleException(ApplicationErrorMessages.IdentityProvider.LoginTypeNotSupported)
        };
    }
}
