namespace PulseCrm.Api.Configuration;

public sealed class CorsOptions
{
    public const string Section = "Cors";

    public string[] AllowedOrigins { get; set; } = ["http://localhost:5173"];
}
