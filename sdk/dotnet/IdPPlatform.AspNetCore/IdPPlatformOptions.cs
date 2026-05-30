namespace IdPPlatform.AspNetCore;

public sealed class IdPPlatformOptions
{
    public const string SectionName = "IdP";

    public string Authority { get; set; } = "http://localhost:5000";

    public string Audience { get; set; } = "idpplatform-api";

    /// <summary>
    /// Accepts self-signed IdP certificates (development only).
    /// </summary>
    public bool AllowInvalidCertificate { get; set; }
}
