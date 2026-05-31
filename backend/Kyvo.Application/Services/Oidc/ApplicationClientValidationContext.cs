namespace Kyvo.Application.Services.Oidc;

public sealed class ApplicationClientValidationContext
{
    public required Domain.Entities.ApplicationClient Client { get; init; }

    public required IReadOnlyList<string> Scopes { get; init; }
}
