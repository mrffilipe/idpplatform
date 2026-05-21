using IdPPlatform.Domain.Enums;

namespace IdPPlatform.Application.Services.AppService;

public sealed record ApplicationDto
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required string Slug { get; init; }

    public required ApplicationType Type { get; init; }

    public required bool IsSystem { get; init; }
}
