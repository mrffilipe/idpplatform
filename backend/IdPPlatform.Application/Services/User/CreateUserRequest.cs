namespace IdPPlatform.Application.Services.Users;

public sealed record CreateUserRequest
{
    public required string Email { get; init; }

    public required string DisplayName { get; init; }

    public string? PhotoUrl { get; init; }
}
