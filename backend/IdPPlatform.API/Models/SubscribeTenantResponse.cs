using IdPPlatform.Application.Services.Auth;
using IdPPlatform.Application.Services.Oidc;

namespace IdPPlatform.API.Models;

/// <summary>
/// Response for SaaS tenant subscription, optionally including freshly issued OAuth tokens for the active session.
/// </summary>
public sealed record SubscribeTenantResponse
{
    public required Guid UserId { get; init; }

    public required string Email { get; init; }

    public Guid? TenantId { get; init; }

    public Guid? MembershipId { get; init; }

    public IReadOnlyList<string> TenantRoles { get; init; } = [];

    public IReadOnlyList<string> PlatformRoles { get; init; } = [];

    public IReadOnlyList<AuthTenantSummaryDto> Tenants { get; init; } = [];

    /// <summary>
    /// Optional OAuth token response (RFC 6749 snake_case) when issued for the active session.
    /// </summary>
    public OidcTokenResponse? Tokens { get; init; }
}
