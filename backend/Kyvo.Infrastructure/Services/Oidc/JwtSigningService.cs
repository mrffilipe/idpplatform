using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using Kyvo.Application.Exceptions;
using Kyvo.Application.Services.Oidc;
using Kyvo.Infrastructure.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Kyvo.Infrastructure.Services.Oidc;

public sealed class JwtSigningService : IJwtSigningService
{
    private readonly JwtOptions _options;
    private readonly RsaSecurityKey _signingKey;
    private readonly SigningCredentials _credentials;
    private readonly string _jwksJson;

    public JwtSigningService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
        var rsa = LoadRsaKey(_options);
        _signingKey = new RsaSecurityKey(rsa) { KeyId = _options.KeyId };
        _credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.RsaSha256);
        _jwksJson = BuildJwksJson(_signingKey, _options.KeyId);
    }

    public SecurityKey SigningKey => _signingKey;

    public string KeyId => _options.KeyId;

    public string Issuer => _options.Issuer.TrimEnd('/');

    public string Audience => _options.Audience;

    public string SignAccessToken(IReadOnlyList<Claim> claims, TimeSpan lifetime)
        => SignToken(claims, lifetime, audience: Audience);

    public string SignIdToken(IReadOnlyList<Claim> claims, TimeSpan lifetime, string audience, string? nonce = null)
    {
        var idClaims = claims.ToList();
        if (!string.IsNullOrWhiteSpace(nonce))
        {
            idClaims.Add(new Claim("nonce", nonce));
        }

        return SignToken(idClaims, lifetime, audience: audience, tokenUse: "id");
    }

    public string GetJwksJson() => _jwksJson;

    private string SignToken(
        IReadOnlyList<Claim> claims,
        TimeSpan lifetime,
        string? audience,
        string? tokenUse = null)
    {
        var now = DateTime.UtcNow;
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Subject = new ClaimsIdentity(claims),
            Expires = now.Add(lifetime),
            NotBefore = now,
            IssuedAt = now,
            SigningCredentials = _credentials,
            Audience = audience
        };

        if (!string.IsNullOrWhiteSpace(tokenUse))
        {
            descriptor.Subject!.AddClaim(new Claim("token_use", tokenUse));
        }

        var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
        return handler.WriteToken(handler.CreateToken(descriptor));
    }

    private static RSA LoadRsaKey(JwtOptions options)
    {
        var pem = !string.IsNullOrWhiteSpace(options.SigningKeyPem)
            ? options.SigningKeyPem
            : File.ReadAllText(options.SigningKeyPath!);

        var rsa = RSA.Create();
        rsa.ImportFromPem(pem);
        return rsa;
    }

    private static string BuildJwksJson(RsaSecurityKey key, string keyId)
    {
        var parameters = key.Rsa ?? throw new InvalidOperationException(ApplicationErrorMessages.Signing.RsaKeyNotAvailable);
        var export = parameters.ExportParameters(false);

        var jwk = new Dictionary<string, object>
        {
            ["kty"] = "RSA",
            ["use"] = "sig",
            ["kid"] = keyId,
            ["alg"] = SecurityAlgorithms.RsaSha256,
            ["n"] = Base64UrlEncoder.Encode(export.Modulus!),
            ["e"] = Base64UrlEncoder.Encode(export.Exponent!)
        };

        return JsonSerializer.Serialize(new { keys = new[] { jwk } });
    }
}
