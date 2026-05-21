namespace IdPPlatform.Infrastructure.Configurations;

public sealed class JwtOptions
{
    public const string Section = "Jwt";

    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public int AccessTokenMinutes { get; init; } = 15;
    public int RefreshTokenDays { get; init; } = 30;
    public string? SigningKeyPath { get; init; }
    public string? SigningKeyPem { get; init; }
    public string KeyId { get; init; } = "default";
}
