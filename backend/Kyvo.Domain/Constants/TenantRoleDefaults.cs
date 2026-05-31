namespace Kyvo.Domain.Constants;

public static class TenantRoleDefaults
{
    public const string Owner = "owner";
    public const string Admin = "admin";
    public const string Member = "member";
    public const string Viewer = "viewer";

    public static readonly IReadOnlyList<TenantRoleDefinition> All =
    [
        new() { Key = Owner, Name = "Owner" },
        new() { Key = Admin, Name = "Admin" },
        new() { Key = Member, Name = "Member" },
        new() { Key = Viewer, Name = "Viewer" }
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
