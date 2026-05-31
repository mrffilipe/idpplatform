using Kyvo.Domain.Enums;

namespace Kyvo.Application.Services.AppService;

public sealed record CreateApplicationClientRequest
{
    public Guid ApplicationId { get; init; }

    public required string ClientId { get; init; }

    public string? ClientSecretHash { get; init; }

    public required ClientType ClientType { get; init; }

    public required string RedirectUris { get; init; }

    public required string AllowedScopes { get; init; }

    public required int AccessTokenTtlSeconds { get; init; }

    public IReadOnlyList<string> ActorPlatformRoles { get; init; } = [];
}
