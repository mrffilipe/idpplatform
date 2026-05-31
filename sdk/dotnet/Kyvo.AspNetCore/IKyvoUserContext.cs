namespace Kyvo.AspNetCore;

public interface IKyvoUserContext
{
    Guid? UserId { get; }

    Guid? TenantId { get; }

    Guid? MembershipId { get; }

    string? Email { get; }

    IReadOnlyList<string> TenantRoles { get; }

    IReadOnlyList<string> PlatformRoles { get; }

    bool HasTenantRole(params string[] roles);

    bool HasPlatformRole(params string[] roles);
}
