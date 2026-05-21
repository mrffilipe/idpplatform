namespace IdPPlatform.Application.Services.Oidc;

public sealed class OidcTokenRequest
{
    public required string GrantType { get; init; }
    public string? Code { get; init; }
    public string? RedirectUri { get; init; }
    public string? ClientId { get; init; }
    public string? ClientSecret { get; init; }
    public string? CodeVerifier { get; init; }
    public string? RefreshToken { get; init; }
    public string? Scope { get; init; }
}
