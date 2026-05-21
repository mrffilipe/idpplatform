using IdPPlatform.Application.Common;

namespace IdPPlatform.Application.Services.AuditLog;

public sealed record ListAuditLogsRequest : PagedRequest
{
    public Guid? UserId { get; init; }

    public string? Action { get; init; }

    public string? ResourceType { get; init; }

    public DateTime? From { get; init; }

    public DateTime? To { get; init; }
}
