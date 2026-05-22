using IdPPlatform.Application.Services.IdentityProvider;
using IdPPlatform.Domain.Enums;
using IdPPlatform.Infrastructure.Services.ExternalIdentityProvider;

namespace IdPPlatform.API.Common;

/// <summary>
/// Shapes the public-safe view of an identity provider config to be rendered on the login page.
/// Currently exposes Firebase web client config only (projectId, webApiKey, authDomain); other types
/// are not surfaced because their login flow is not yet implemented.
/// </summary>
public static class FederatedProviderClientConfig
{
    public static IReadOnlyDictionary<string, string>? Build(
        IdentityProviderType providerType,
        string? decryptedConfigJson) => providerType switch
        {
            IdentityProviderType.Firebase => BuildFirebase(decryptedConfigJson),
            _ => null
        };

    private static IReadOnlyDictionary<string, string>? BuildFirebase(string? configJson)
    {
        try
        {
            var config = FirebaseTokenValidator.DeserializeConfig(configJson);
            if (string.IsNullOrWhiteSpace(config.ProjectId) || string.IsNullOrWhiteSpace(config.WebApiKey))
            {
                return null;
            }

            return new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["projectId"] = config.ProjectId,
                ["webApiKey"] = config.WebApiKey,
                ["authDomain"] = config.ResolveAuthDomain()
            };
        }
        catch
        {
            return null;
        }
    }
}
