namespace Kyvo.Infrastructure.Configurations;

public sealed class DatabaseOptions
{
    public const string Section = "Database";

    public required string ConnectionString { get; init; }
}
