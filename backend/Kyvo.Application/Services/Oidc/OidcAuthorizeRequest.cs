namespace Kyvo.Application.Services.Oidc;

public sealed class OidcAuthorizeRequest
{
    public required string ClientId { get; init; }

    public required string RedirectUri { get; init; }

    public required string ResponseType { get; init; }

    public required string Scope { get; init; }

    public string? State { get; init; }

    public string? Prompt { get; init; }

    public int? MaxAge { get; init; }

    public string? CodeChallenge { get; init; }

    public string? CodeChallengeMethod { get; init; }

    public string? Nonce { get; init; }
}
