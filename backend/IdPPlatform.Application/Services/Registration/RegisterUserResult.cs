namespace IdPPlatform.Application.Services.Registration;

public sealed record RegisterUserResult
{
    public required Guid UserId { get; init; }

    public required string Email { get; init; }

    public required string DisplayName { get; init; }
}
