using Kyvo.Domain.Enums;

namespace Kyvo.Application.Services.IdentityProvider;

public interface IIdentityProviderTokenValidatorFactory
{
    IIdentityProviderTokenValidator GetValidator(IdentityProviderType providerType);
}
