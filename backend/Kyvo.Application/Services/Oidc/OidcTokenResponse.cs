using System.Text.Json.Serialization;

namespace Kyvo.Application.Services.Oidc;

/// <summary>
/// OAuth 2.0 access token response (RFC 6749 Section 5.1). Property names use snake_case per the standard.
/// </summary>
public sealed class OidcTokenResponse
{
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; init; }

    [JsonPropertyName("token_type")]
    public required string TokenType { get; init; }

    [JsonPropertyName("expires_in")]
    public required int ExpiresIn { get; init; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; init; }

    [JsonPropertyName("id_token")]
    public string? IdToken { get; init; }

    [JsonPropertyName("scope")]
    public string? Scope { get; init; }
}
