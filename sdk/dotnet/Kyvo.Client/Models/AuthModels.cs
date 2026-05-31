namespace Kyvo.Client.Models;

public sealed record SubscribeTenantRequest(
    string TenantName,
    string TenantKey,
    string? PlanCode = null,
    string? ExternalCustomerId = null);

public sealed record AuthTenantSummaryDto(
    Guid TenantId,
    string TenantName,
    string TenantKey,
    IReadOnlyList<string> Roles);

public sealed record TenantContextResult(
    Guid UserId,
    string Email,
    Guid? TenantId,
    Guid? MembershipId,
    IReadOnlyList<string> TenantRoles,
    IReadOnlyList<string> PlatformRoles,
    IReadOnlyList<AuthTenantSummaryDto> Tenants);

public sealed record SubscribeTenantResponse(
    Guid UserId,
    string Email,
    Guid? TenantId,
    Guid? MembershipId,
    IReadOnlyList<string> TenantRoles,
    IReadOnlyList<string> PlatformRoles,
    IReadOnlyList<AuthTenantSummaryDto> Tenants,
    OidcTokenResponse? Tokens);

public sealed record SubscribeTenantResult(
    TenantContextResult Context,
    OidcTokenResponse? Tokens);

public sealed record SwitchTenantRequest(Guid TenantId);

public sealed record AuthSessionDto(
    Guid Id,
    DateTime CreatedAt,
    DateTime? LastSeenAt,
    string? UserAgent,
    string? IpAddress,
    bool IsCurrent);
