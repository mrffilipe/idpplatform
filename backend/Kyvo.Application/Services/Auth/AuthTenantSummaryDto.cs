namespace Kyvo.Application.Services.Auth;

public sealed record AuthTenantSummaryDto
{
    public required Guid TenantId { get; init; }

    public required string TenantName { get; init; }

    public required string TenantKey { get; init; }

    public required IReadOnlyList<string> Roles { get; init; }
}
