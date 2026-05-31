namespace Kyvo.Application.Services.UserScope;

public interface IUserScope
{
    bool IsAuthenticated { get; }

    Guid UserId { get; }

    Guid? SessionId { get; }

    Guid? TenantId { get; }

    Guid? MembershipId { get; }

    IReadOnlyList<string> TenantRoles { get; }

    IReadOnlyList<string> PlatformRoles { get; }

    bool HasAnyTenantRole(params string[] roleKeys);

    bool HasAnyPlatformRole(params string[] roleKeys);
}
