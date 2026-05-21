using IdPPlatform.Domain.Enums;

namespace IdPPlatform.Application.Services.AppService;

public sealed record CreateApplicationRequest
{
    public required string Name { get; init; }

    public required string Slug { get; init; }

    public required ApplicationType Type { get; init; }
}
