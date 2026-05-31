namespace Kyvo.Application.Services.Registration;

public sealed record RegisterUserRequest
{
    public required string Email { get; init; }

    public required string Password { get; init; }

    public required string DisplayName { get; init; }
}
