namespace Kyvo.AspNetCore;

public sealed class KyvoOptions
{
    public const string SectionName = "Kyvo";

    public string Authority { get; set; } = "http://localhost:5000";

    public string Audience { get; set; } = "kyvo-api";

    /// <summary>
    /// Accepts self-signed Kyvo certificates (development only).
    /// </summary>
    public bool AllowInvalidCertificate { get; set; }
}
