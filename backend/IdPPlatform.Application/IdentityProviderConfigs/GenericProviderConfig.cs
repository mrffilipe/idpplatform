namespace IdPPlatform.Application.IdentityProviderConfigs;

public sealed class GenericProviderConfig
{
    public string Issuer { get; init; } = string.Empty;

    public string JwksUri { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;
}
