using System.Text.Json;
using IdPPlatform.Application.Exceptions;
using IdPPlatform.Application.IdentityProviderConfigs;
using IdPPlatform.Application.Services.IdentityProvider;
using IdPPlatform.Domain.Enums;
using IdPPlatform.Domain.Exceptions;

namespace IdPPlatform.Infrastructure.Services.ExternalIdentityProvider;

public sealed class IdentityProviderConfigValidator : IIdentityProviderConfigValidator
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public void ValidateForSave(IdentityProviderType providerType, string? configJson)
    {
        if (providerType == IdentityProviderType.Local)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(configJson))
        {
            throw new DomainValidationException(ApplicationErrorMessages.IdentityProvider.ConfigRequired);
        }

        try
        {
            switch (providerType)
            {
                case IdentityProviderType.Firebase:
                    ValidateFirebase(configJson);
                    break;
                case IdentityProviderType.Cognito:
                    ValidateCognito(configJson);
                    break;
                case IdentityProviderType.Generic:
                    ValidateGeneric(configJson);
                    break;
                default:
                    throw new DomainValidationException(ApplicationErrorMessages.IdentityProvider.ConfigInvalid);
            }
        }
        catch (JsonException)
        {
            throw new DomainValidationException(ApplicationErrorMessages.IdentityProvider.ConfigInvalid);
        }
    }

    private static void ValidateFirebase(string configJson)
    {
        var config = JsonSerializer.Deserialize<FirebaseProviderConfig>(configJson, JsonOptions)
            ?? throw new DomainValidationException(ApplicationErrorMessages.IdentityProvider.ConfigInvalid);

        if (string.IsNullOrWhiteSpace(config.ProjectId)
            || string.IsNullOrWhiteSpace(config.WebApiKey)
            || config.ServiceAccount.ValueKind == JsonValueKind.Undefined
            || config.ServiceAccount.ValueKind == JsonValueKind.Null)
        {
            throw new DomainValidationException(ApplicationErrorMessages.IdentityProvider.ConfigInvalid);
        }
    }

    private static void ValidateCognito(string configJson)
    {
        var config = JsonSerializer.Deserialize<CognitoProviderConfig>(configJson, JsonOptions)
            ?? throw new DomainValidationException(ApplicationErrorMessages.IdentityProvider.ConfigInvalid);

        if (string.IsNullOrWhiteSpace(config.UserPoolId)
            || string.IsNullOrWhiteSpace(config.Region)
            || string.IsNullOrWhiteSpace(config.ClientId))
        {
            throw new DomainValidationException(ApplicationErrorMessages.IdentityProvider.ConfigInvalid);
        }
    }

    private static void ValidateGeneric(string configJson)
    {
        var config = JsonSerializer.Deserialize<GenericProviderConfig>(configJson, JsonOptions)
            ?? throw new DomainValidationException(ApplicationErrorMessages.IdentityProvider.ConfigInvalid);

        if (string.IsNullOrWhiteSpace(config.Issuer)
            || string.IsNullOrWhiteSpace(config.JwksUri)
            || string.IsNullOrWhiteSpace(config.Audience))
        {
            throw new DomainValidationException(ApplicationErrorMessages.IdentityProvider.ConfigInvalid);
        }
    }
}
