namespace Kyvo.Application.Services.Users;

public sealed record GetUserByIdRequest
{
    public required Guid UserId { get; init; }
}
