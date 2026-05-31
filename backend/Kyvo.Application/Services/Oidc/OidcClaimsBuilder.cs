using System.Security.Claims;
using Kyvo.Application.Services.Auth;
using Kyvo.Domain.Constants;

namespace Kyvo.Application.Services.Oidc;

public static class OidcClaimsBuilder
{
    public static IReadOnlyList<Claim> Build(
        ExternalLoginResult login,
        Guid sessionId,
        Guid? tenantId,
        Guid? membershipId,
        IReadOnlyList<string> tenantRoles)
    {
        var claims = new List<Claim>
        {
            new(OidcConstants.Claims.Subject, login.UserId.ToString("D")),
            new("uid", login.UserId.ToString("D")),
            new("sid", sessionId.ToString("D")),
            new(OidcConstants.Claims.Email, login.Email),
            new(OidcConstants.Claims.Name, login.DisplayName),
            new("amr", "pwd")
        };

        if (tenantId.HasValue)
        {
            claims.Add(new Claim("tid", tenantId.Value.ToString("D")));
        }

        if (membershipId.HasValue)
        {
            claims.Add(new Claim("mid", membershipId.Value.ToString("D")));
        }

        foreach (var role in tenantRoles
                     .Select(x => x.Trim().ToLowerInvariant())
                     .Where(x => x.Length > 0)
                     .Distinct())
        {
            claims.Add(new Claim("trole", role));
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        foreach (var role in login.PlatformRoles
                     .Select(x => x.Trim().ToLowerInvariant())
                     .Where(x => x.Length > 0)
                     .Distinct())
        {
            claims.Add(new Claim(PlatformRoleDefaults.ClaimType, role));
        }

        return claims;
    }

    public static IReadOnlyList<Claim> ForAccessToken(IReadOnlyList<Claim> allClaims) => allClaims;

    public static IReadOnlyList<Claim> ForIdToken(IReadOnlyList<Claim> allClaims)
    {
        var idTokenTypes = new HashSet<string>(StringComparer.Ordinal)
        {
            OidcConstants.Claims.Subject,
            OidcConstants.Claims.Name,
            OidcConstants.Claims.Email,
            "uid",
            "sid",
            "tid",
            "mid",
            "trole",
            PlatformRoleDefaults.ClaimType,
            "amr"
        };

        return allClaims.Where(c => idTokenTypes.Contains(c.Type)).ToList();
    }
}
