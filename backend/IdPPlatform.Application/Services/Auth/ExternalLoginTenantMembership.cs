namespace IdPPlatform.Application.Services.Auth;

public sealed class ExternalLoginTenantMembership
{
    public required Guid TenantId { get; init; }

    public required Guid MembershipId { get; init; }

    public required IReadOnlyList<string> Roles { get; init; }
}
