namespace IdPPlatform.Infrastructure.Configurations;

public sealed class BootstrapOptions
{
    public const string Section = "Bootstrap";

    /// <summary>
    /// Nome da variável de ambiente que define o email do admin raiz.
    /// Alternativa: definir <c>Bootstrap:AdminEmail</c> no appsettings (sobrescrito pela env var se ambos presentes).
    /// </summary>
    public const string AdminEmailEnvVar = "PLATFORM_BOOTSTRAP_ADMIN_EMAIL";

    /// <summary>
    /// Nome da variável de ambiente que define a senha do admin raiz.
    /// Recomendado usar apenas via env var em produção; nunca persistir em texto no appsettings de produção.
    /// Alternativa para desenvolvimento local: definir <c>Bootstrap:AdminPassword</c> no appsettings.Development.json.
    /// </summary>
    public const string AdminPasswordEnvVar = "PLATFORM_BOOTSTRAP_ADMIN_PASSWORD";

    /// <summary>
    /// Nome da variável de ambiente que define o display name do admin raiz (opcional).
    /// Alternativa: definir <c>Bootstrap:AdminDisplayName</c> no appsettings.
    /// </summary>
    public const string AdminDisplayNameEnvVar = "PLATFORM_BOOTSTRAP_ADMIN_DISPLAY_NAME";

    public string? AdminEmail { get; set; }

    public string? AdminPassword { get; set; }

    public string? AdminDisplayName { get; set; }
}
