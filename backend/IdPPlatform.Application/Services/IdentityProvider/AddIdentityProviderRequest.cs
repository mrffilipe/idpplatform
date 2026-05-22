using IdPPlatform.Domain.Enums;

namespace IdPPlatform.Application.Services.IdentityProvider;

public sealed record AddIdentityProviderRequest
{
    public required string Alias { get; init; }

    public required string DisplayName { get; init; }

    public required IdentityProviderType ProviderType { get; init; }

    public required IReadOnlyCollection<IdpCapability> Capabilities { get; init; }

    public string? ConfigJson { get; init; }
}
