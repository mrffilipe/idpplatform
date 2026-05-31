using System.Text.Json.Serialization;

namespace Kyvo.API.Models.Oidc;

/// <summary>
/// OpenID Connect UserInfo response (standard claim names).
/// </summary>
public sealed class OidcUserInfoResponse
{
    [JsonPropertyName("sub")]
    public string? Sub { get; init; }

    [JsonPropertyName("email")]
    public string? Email { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("tid")]
    public string? Tid { get; init; }

    [JsonPropertyName("mid")]
    public string? Mid { get; init; }

    [JsonPropertyName("trole")]
    public IReadOnlyList<string>? Trole { get; init; }

    [JsonPropertyName("prole")]
    public string? Prole { get; init; }
}
