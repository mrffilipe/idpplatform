namespace Kyvo.Client.Models;

public sealed record AuditLogItemDto(
    Guid Id,
    string Action,
    string? ActorUserId,
    string? ActorEmail,
    DateTime OccurredAt,
    string? ResourceType,
    string? ResourceId,
    string? Details);

public sealed record ListAuditLogsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Action = null,
    DateTime? From = null,
    DateTime? To = null);
