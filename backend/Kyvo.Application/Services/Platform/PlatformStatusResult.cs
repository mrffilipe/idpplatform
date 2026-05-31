namespace Kyvo.Application.Services.Platform;

public sealed record PlatformStatusResult
{
    public required bool IsConfigured { get; init; }

    public required bool RequiresBootstrap { get; init; }

    public string? OauthClientId { get; init; }
}
