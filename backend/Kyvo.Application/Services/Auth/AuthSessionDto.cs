using Kyvo.Domain.Enums;

namespace Kyvo.Application.Services.Auth;

public sealed record AuthSessionDto
{
    public required Guid SessionId { get; init; }

    public Guid? TenantId { get; init; }

    public Guid? MembershipId { get; init; }

    public Guid? ClientId { get; init; }

    public required SessionStatus Status { get; init; }

    public string? UserAgent { get; init; }

    public string? IpAddress { get; init; }

    public required DateTime ExpiresAt { get; init; }

    public required DateTime LastActivityAt { get; init; }
}
