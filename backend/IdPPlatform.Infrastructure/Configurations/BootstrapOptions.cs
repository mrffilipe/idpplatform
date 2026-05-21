namespace IdPPlatform.Infrastructure.Configurations;

public sealed class BootstrapOptions
{
    public const string Section = "Bootstrap";

    /// <summary>
    /// Variável de ambiente (Docker / .env): mapeia para <c>Bootstrap:AdminEmail</c>.
    /// Em appsettings JSON use a seção aninhada <c>Bootstrap</c> com <c>AdminEmail</c>.
    /// </summary>
    public const string AdminEmailEnvVar = "Bootstrap__AdminEmail";

    /// <summary>
    /// Variável de ambiente (Docker / .env): mapeia para <c>Bootstrap:AdminPassword</c>.
    /// Recomendado usar apenas via env var em produção; nunca commitar senha real no appsettings.
    /// </summary>
    public const string AdminPasswordEnvVar = "Bootstrap__AdminPassword";

    /// <summary>
    /// Variável de ambiente (Docker / .env): mapeia para <c>Bootstrap:AdminDisplayName</c> (opcional).
    /// </summary>
    public const string AdminDisplayNameEnvVar = "Bootstrap__AdminDisplayName";

    public string? AdminEmail { get; set; }

    public string? AdminPassword { get; set; }

    public string? AdminDisplayName { get; set; }
}
