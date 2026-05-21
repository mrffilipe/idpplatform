namespace PulseCrm.Api.Services;

public sealed record SubscribeTenantRequest(
    string TenantName,
    string TenantKey,
    string? PlanCode,
    string? ExternalCustomerId);

public sealed record IdPTenantSummary(
    Guid TenantId,
    string TenantName,
    string TenantKey,
    IReadOnlyList<string> Roles);

public sealed record IdPTenantContextResult(
    Guid UserId,
    string Email,
    Guid? TenantId,
    Guid? MembershipId,
    IReadOnlyList<string> TenantRoles,
    IReadOnlyList<string> PlatformRoles,
    IReadOnlyList<IdPTenantSummary> Tenants);

public sealed record IdPOidcTokenPayload(
    string AccessToken,
    string? RefreshToken,
    int ExpiresIn,
    string TokenType);

public sealed record IdPSubscribeResult(
    IdPTenantContextResult Context,
    IdPOidcTokenPayload? Tokens);
