namespace Kyvo.Application.Services.Auth;

public sealed record TenantContextResult
{
    public required Guid UserId { get; init; }

    public required string Email { get; init; }

    public Guid? TenantId { get; init; }

    public Guid? MembershipId { get; init; }

    public IReadOnlyList<string> TenantRoles { get; init; } = [];

    public IReadOnlyList<string> PlatformRoles { get; init; } = [];

    public IReadOnlyList<AuthTenantSummaryDto> Tenants { get; init; } = [];
}
