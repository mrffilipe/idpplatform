namespace IdPPlatform.Domain.Constants;

public static class TenantRoleDefaults
{
    public const string Owner = "owner";
    public const string Admin = "admin";
    public const string Member = "member";
    public const string Viewer = "viewer";

    public static readonly IReadOnlyList<TenantRoleDefinition> All =
    [
        new(Owner, "Owner"),
        new(Admin, "Admin"),
        new(Member, "Member"),
        new(Viewer, "Viewer")
    ];

    public static readonly IReadOnlySet<string> AdministrativeKeys =
        new HashSet<string>([Owner, Admin], StringComparer.OrdinalIgnoreCase);

    public static string FromLegacyRole(int role)
    {
        return role switch
        {
            0 => Owner,
            1 => Admin,
            2 => Member,
            3 => Viewer,
            _ => Viewer
        };
    }
}

public sealed record TenantRoleDefinition(string Key, string Name);
