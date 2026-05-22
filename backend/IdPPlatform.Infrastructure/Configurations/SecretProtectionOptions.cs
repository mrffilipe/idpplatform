namespace IdPPlatform.Infrastructure.Configurations;

/// <summary>
/// Configuration for the ASP.NET Core Data Protection keyring used to encrypt
/// sensitive identity provider configuration values at rest.
/// </summary>
public sealed class SecretProtectionOptions
{
    public const string Section = "SecretProtection";

    /// <summary>
    /// Filesystem directory where the data protection key ring is persisted.
    /// In production this should be a persistent, app-only directory backed up alongside the database
    /// (lose the keys and previously encrypted secrets become unreadable).
    /// </summary>
    public string KeyDirectoryPath { get; init; } = "keys/data-protection";

    /// <summary>
    /// Application name used to isolate the key ring from other applications sharing the same directory.
    /// </summary>
    public string ApplicationName { get; init; } = "IdPPlatform";
}
