namespace Kyvo.Application.Services.Users;

public sealed record UpdateUserProfileRequest
{
    public Guid UserId { get; init; }

    public required string DisplayName { get; init; }

    public string? PhotoUrl { get; init; }
}
