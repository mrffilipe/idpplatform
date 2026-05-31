namespace Kyvo.Application.Services.AuditLog;

public sealed record AuditLogItemDto
{
    public required Guid Id { get; init; }

    public required Guid TenantId { get; init; }

    public Guid? UserId { get; init; }

    public Guid? MembershipId { get; init; }

    public required string Action { get; init; }

    public required string ResourceType { get; init; }

    public Guid? ResourceId { get; init; }

    public string? IpAddress { get; init; }

    public string? UserAgent { get; init; }

    public required DateTime CreatedAt { get; init; }
}
