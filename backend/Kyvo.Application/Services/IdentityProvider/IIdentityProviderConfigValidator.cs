using Kyvo.Domain.Enums;

namespace Kyvo.Application.Services.IdentityProvider;

public interface IIdentityProviderConfigValidator
{
    void ValidateForSave(IdentityProviderType providerType, string? configJson);
}
