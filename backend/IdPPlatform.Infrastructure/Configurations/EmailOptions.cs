namespace IdPPlatform.Infrastructure.Configurations;

public sealed class EmailOptions
{
    public const string Section = "Email";

    public string FromAddress { get; init; } = string.Empty;

    public string Region { get; init; } = "us-east-1";

    public string? AccessKeyId { get; init; }

    public string? SecretAccessKey { get; init; }

    public string? SessionToken { get; init; }
}
