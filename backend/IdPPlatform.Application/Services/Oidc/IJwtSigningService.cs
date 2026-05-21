using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace IdPPlatform.Application.Services.Oidc;

public interface IJwtSigningService
{
    SecurityKey SigningKey { get; }

    string KeyId { get; }

    string Issuer { get; }

    string Audience { get; }

    string SignAccessToken(IReadOnlyList<Claim> claims, TimeSpan lifetime);

    string SignIdToken(
        IReadOnlyList<Claim> claims,
        TimeSpan lifetime,
        string audience,
        string? nonce = null);

    string GetJwksJson();
}
