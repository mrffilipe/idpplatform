using Microsoft.AspNetCore.Mvc;

namespace Kyvo.API.Models.Oidc;

/// <summary>
/// Form body for local email/password sign-in (application/x-www-form-urlencoded).
/// </summary>
public sealed class AccountLoginForm
{
    [FromForm]
    public string Email { get; set; } = string.Empty;

    [FromForm]
    public string Password { get; set; } = string.Empty;

    [FromForm]
    public string? ReturnUrl { get; set; }
}
