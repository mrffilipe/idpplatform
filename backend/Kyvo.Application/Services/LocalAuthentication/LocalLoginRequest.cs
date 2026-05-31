namespace Kyvo.Application.Services.LocalAuthentication;

public sealed record LocalLoginRequest
{
    public required string Email { get; init; }

    public required string Password { get; init; }
}
