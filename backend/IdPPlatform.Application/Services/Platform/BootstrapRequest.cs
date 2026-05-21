namespace IdPPlatform.Application.Services.Platform;

public sealed record BootstrapRequest
{
    public string? UserAgent { get; init; }

    public string? IpAddress { get; init; }
}
