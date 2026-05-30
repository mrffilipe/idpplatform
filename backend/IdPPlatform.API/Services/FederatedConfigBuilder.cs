using IdPPlatform.Domain.Enums;
using IdPPlatform.Infrastructure.Services.ExternalIdentityProvider;

namespace IdPPlatform.API.Services;

/// <inheritdoc />
public sealed class FederatedConfigBuilder : IFederatedConfigBuilder
{
    private readonly ILogger<FederatedConfigBuilder> _logger;

    public FederatedConfigBuilder(ILogger<FederatedConfigBuilder> logger) => _logger = logger;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string>? Build(IdentityProviderType providerType, string? decryptedConfigJson)
        => providerType switch
        {
            IdentityProviderType.Firebase => BuildFirebase(decryptedConfigJson),
            _ => null
        };

    private IReadOnlyDictionary<string, string>? BuildFirebase(string? configJson)
    {
        if (string.IsNullOrWhiteSpace(configJson))
        {
            _logger.LogWarning("Firebase provider config is missing or empty; federated login button will be hidden.");
            return null;
        }

        try
        {
            var config = FirebaseTokenValidator.DeserializeConfig(configJson);
            if (string.IsNullOrWhiteSpace(config.ProjectId) || string.IsNullOrWhiteSpace(config.WebApiKey))
            {
                _logger.LogWarning(
                    "Firebase provider config is incomplete (projectId or webApiKey missing); federated login button will be hidden.");
                return null;
            }

            return new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["projectId"] = config.ProjectId,
                ["webApiKey"] = config.WebApiKey,
                ["authDomain"] = config.ResolveAuthDomain()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build Firebase client config for login page; federated login button will be hidden.");
            return null;
        }
    }
}
