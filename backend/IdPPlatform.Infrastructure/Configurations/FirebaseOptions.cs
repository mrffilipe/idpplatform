namespace IdPPlatform.Infrastructure.Configurations;

public sealed class FirebaseOptions
{
    public const string Section = "Firebase";

    public string ProjectId { get; init; } = string.Empty;
    public string? CredentialPath { get; init; }
    public string WebApiKey { get; init; } = string.Empty;
}
