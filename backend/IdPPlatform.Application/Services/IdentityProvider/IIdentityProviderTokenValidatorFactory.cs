using IdPPlatform.Domain.Enums;

namespace IdPPlatform.Application.Services.IdentityProvider;

public interface IIdentityProviderTokenValidatorFactory
{
    IIdentityProviderTokenValidator GetValidator(IdentityProviderType providerType);
}
