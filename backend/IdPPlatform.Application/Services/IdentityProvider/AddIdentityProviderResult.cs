namespace IdPPlatform.Application.Services.IdentityProvider;

/// <summary>
/// Result returned when an identity provider is created. <see cref="Warnings"/> reports soft conflicts
/// (e.g., a social capability already advertised by another enabled provider) that do not block creation.
/// </summary>
public sealed record AddIdentityProviderResult
{
    public required Guid Id { get; init; }

    public required IReadOnlyList<string> Warnings { get; init; }
}
