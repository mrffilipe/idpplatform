using IdPPlatform.Domain.Enums;

namespace IdPPlatform.Application.Services.IdentityProvider;

public sealed record IdentityProviderDto
{
    public required Guid Id { get; init; }

    public required string Alias { get; init; }

    public required string DisplayName { get; init; }

    public required IdentityProviderType ProviderType { get; init; }

    public required bool Enabled { get; init; }

    public required IReadOnlyCollection<IdpCapability> Capabilities { get; init; }
}
