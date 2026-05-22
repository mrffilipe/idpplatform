namespace IdPPlatform.Infrastructure.Configurations;

public sealed class RateLimitOptions
{
    public const string Section = "RateLimit";

    public int BootstrapPermitLimit { get; init; } = 3;

    public int BootstrapWindowMinutes { get; init; } = 15;

    public int AccountRegisterPermitLimit { get; init; } = 5;

    public int AccountRegisterWindowMinutes { get; init; } = 15;
}
