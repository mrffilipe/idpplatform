using IdPPlatform.Domain.Enums;

namespace IdPPlatform.API.Services;

/// <summary>
/// Builds public-safe client configuration for federated login providers (e.g. Firebase web SDK).
/// </summary>
public interface IFederatedConfigBuilder
{
    IReadOnlyDictionary<string, string>? Build(IdentityProviderType providerType, string? decryptedConfigJson);
}
