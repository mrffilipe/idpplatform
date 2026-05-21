namespace PulseCrm.Api.Services;

public interface IUserContext
{
    Guid? UserId { get; }

    Guid? TenantId { get; }

    Guid? MembershipId { get; }

    string? Email { get; }

    IReadOnlyList<string> TenantRoles { get; }

    IReadOnlyList<string> PlatformRoles { get; }

    IReadOnlyDictionary<string, string> AllClaims { get; }
}
