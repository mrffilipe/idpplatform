namespace Kyvo.Application.Services.Oidc;

public sealed class OidcError
{
    public required string Error { get; init; }
    public string? ErrorDescription { get; init; }
}
