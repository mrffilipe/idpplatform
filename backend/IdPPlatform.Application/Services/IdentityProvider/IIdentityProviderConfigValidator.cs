using IdPPlatform.Domain.Enums;

namespace IdPPlatform.Application.Services.IdentityProvider;

public interface IIdentityProviderConfigValidator
{
    void ValidateForSave(IdentityProviderType providerType, string? configJson);
}
