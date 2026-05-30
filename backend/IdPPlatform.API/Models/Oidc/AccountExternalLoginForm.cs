using Microsoft.AspNetCore.Mvc;

namespace IdPPlatform.API.Models.Oidc;

/// <summary>
/// Form body for federated sign-in (application/x-www-form-urlencoded).
/// </summary>
public sealed class AccountExternalLoginForm
{
    [FromForm]
    public string ProviderAlias { get; set; } = string.Empty;

    [FromForm]
    public string IdToken { get; set; } = string.Empty;

    [FromForm]
    public string? ReturnUrl { get; set; }
}
