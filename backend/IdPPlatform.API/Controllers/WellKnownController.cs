using System.Text.Json;
using IdPPlatform.Application.Services.Oidc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdPPlatform.API.Controllers;

[ApiController]
[AllowAnonymous]
public sealed class WellKnownController : ControllerBase
{
    private readonly IJwtSigningService _signing;

    public WellKnownController(IJwtSigningService signing)
    {
        _signing = signing;
    }

    [HttpGet("/.well-known/openid-configuration")]
    public IActionResult GetOpenIdConfiguration()
    {
        var issuer = _signing.Issuer;
        var document = new Dictionary<string, object>
        {
            ["issuer"] = issuer,
            ["authorization_endpoint"] = $"{issuer}/connect/authorize",
            ["token_endpoint"] = $"{issuer}/connect/token",
            ["userinfo_endpoint"] = $"{issuer}/connect/userinfo",
            ["end_session_endpoint"] = $"{issuer}/connect/logout",
            ["jwks_uri"] = $"{issuer}/.well-known/jwks.json",
            ["response_types_supported"] = new[] { "code" },
            ["grant_types_supported"] = new[] { "authorization_code", "refresh_token" },
            ["subject_types_supported"] = new[] { "public" },
            ["id_token_signing_alg_values_supported"] = new[] { "RS256" },
            ["token_endpoint_auth_methods_supported"] = new[] { "client_secret_post", "client_secret_basic", "none" },
            ["code_challenge_methods_supported"] = new[] { "S256" },
            ["scopes_supported"] = new[]
            {
                OidcConstants.Scopes.OpenId,
                OidcConstants.Scopes.Profile,
                OidcConstants.Scopes.Email,
                OidcConstants.Scopes.OfflineAccess
            }
        };

        return Content(JsonSerializer.Serialize(document), "application/json");
    }

    [HttpGet("/.well-known/jwks.json")]
    public IActionResult GetJwksJson()
    {
        return Content(_signing.GetJwksJson(), "application/json");
    }
}
