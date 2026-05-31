namespace Kyvo.Application.Services.Users;

public sealed record LinkExternalIdentityRequest
{
    public required Guid UserId { get; init; }

    public required string Provider { get; init; }

    public required string ProviderUserId { get; init; }

    public required string Email { get; init; }
}
