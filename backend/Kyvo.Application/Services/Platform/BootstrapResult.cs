namespace Kyvo.Application.Services.Platform;

public sealed record BootstrapResult
{
    public required bool IsConfigured { get; init; }

    public required Guid RootUserId { get; init; }

    public required string OauthClientId { get; init; }
}
