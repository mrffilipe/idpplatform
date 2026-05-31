namespace Kyvo.Infrastructure.Configurations;

public sealed class BootstrapOptions
{
    public const string Section = "Bootstrap";

    /// <summary>
    /// Environment variable (Docker / .env): maps to <c>Bootstrap:AdminEmail</c>.
    /// In appsettings JSON use the nested <c>Bootstrap</c> section with <c>AdminEmail</c>.
    /// </summary>
    public const string AdminEmailEnvVar = "Bootstrap__AdminEmail";

    /// <summary>
    /// Environment variable (Docker / .env): maps to <c>Bootstrap:AdminPassword</c>.
    /// Recommended to use only via env var in production; never commit a real password in appsettings.
    /// </summary>
    public const string AdminPasswordEnvVar = "Bootstrap__AdminPassword";

    /// <summary>
    /// Environment variable (Docker / .env): maps to <c>Bootstrap:AdminDisplayName</c> (optional).
    /// </summary>
    public const string AdminDisplayNameEnvVar = "Bootstrap__AdminDisplayName";

    public string? AdminEmail { get; set; }

    public string? AdminPassword { get; set; }

    public string? AdminDisplayName { get; set; }
}
