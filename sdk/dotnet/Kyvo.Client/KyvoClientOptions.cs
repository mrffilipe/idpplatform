namespace Kyvo.Client;

public sealed class KyvoClientOptions
{
    public const string SectionName = "Kyvo";

    public string Authority { get; set; } = "http://localhost:5000";

    public string ApiVersion { get; set; } = "1.0";

    public bool AllowInvalidCertificate { get; set; }

    public string VersionPrefix => $"/v{ApiVersion.Trim()}";
}
