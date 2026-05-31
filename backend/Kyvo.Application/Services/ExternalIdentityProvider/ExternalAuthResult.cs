namespace Kyvo.Application.Services.ExternalIdentityProvider;

public sealed record ExternalAuthResult
{
    public required string Provider { get; init; }

    public required string ProviderUserId { get; init; }

    public required string Email { get; init; }

    public required bool EmailVerified { get; init; }

    public required IReadOnlyList<string> AuthenticationMethods { get; init; }
}
