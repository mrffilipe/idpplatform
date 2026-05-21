namespace IdPPlatform.Infrastructure.Configurations;

public sealed class SessionOptions
{
    public const string Section = "Session";

    public int MaxSessionsPerUser { get; init; } = 5;
}
