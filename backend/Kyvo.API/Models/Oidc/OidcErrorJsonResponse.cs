using System.Text.Json.Serialization;

namespace Kyvo.API.Models.Oidc;

/// <summary>
/// OAuth 2.0 error response (RFC 6749 Section 5.2).
/// </summary>
public sealed class OidcErrorJsonResponse
{
    [JsonPropertyName("error")]
    public required string Error { get; init; }

    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; init; }
}
