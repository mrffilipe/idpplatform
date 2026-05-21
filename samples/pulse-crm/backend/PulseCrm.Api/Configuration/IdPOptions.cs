namespace PulseCrm.Api.Configuration;

public sealed class IdPOptions
{
    public const string Section = "IdP";

    public string Authority { get; set; } = "http://localhost:5000";

    public string Audience { get; set; } = "idpplatform-api";

    public string ApiVersion { get; set; } = "1.0";

    public string SubscribePath => $"/v{ApiVersion}/auth/subscribe";
}
