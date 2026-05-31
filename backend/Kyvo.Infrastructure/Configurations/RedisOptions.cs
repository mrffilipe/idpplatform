namespace Kyvo.Infrastructure.Configurations;

public sealed class RedisOptions
{
    public const string Section = "Redis";

    public string ConnectionString { get; init; } = string.Empty;

    public string InstanceName { get; init; } = "kyvo:";

    public int TenantIdentifierCacheMinutes { get; init; } = 5;
}
