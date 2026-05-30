using IdPPlatform.Domain.Enums;

namespace IdPPlatform.Application.Services.IdentityProvider;

public sealed record UpdateIdentityProviderRequest
{
    public Guid Id { get; init; }

    public required string DisplayName { get; init; }

    public IReadOnlyCollection<IdpCapability>? Capabilities { get; init; }

    public string? ConfigJson { get; init; }
}
