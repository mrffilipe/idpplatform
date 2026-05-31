namespace Kyvo.Domain.Constants;

public static class PlatformRoleDefaults
{
    public const string ClaimType = "prole";
    public const string PlatformAdministrator = "plat_admin";

    public static readonly IReadOnlySet<string> AdministrativeKeys =
        new HashSet<string>([PlatformAdministrator], StringComparer.OrdinalIgnoreCase);
}
