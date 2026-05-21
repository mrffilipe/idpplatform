namespace IdPPlatform.Application.Services.Users;

public sealed record UserMembershipDto
{
    public required Guid MembershipId { get; init; }

    public required Guid TenantId { get; init; }

    public required string TenantName { get; init; }

    public required string TenantKey { get; init; }

    public required IReadOnlyList<string> Roles { get; init; }
}
