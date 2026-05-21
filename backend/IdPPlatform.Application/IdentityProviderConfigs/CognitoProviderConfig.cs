namespace IdPPlatform.Application.IdentityProviderConfigs;

public sealed class CognitoProviderConfig
{
    public string UserPoolId { get; init; } = string.Empty;

    public string Region { get; init; } = string.Empty;

    public string ClientId { get; init; } = string.Empty;
}
