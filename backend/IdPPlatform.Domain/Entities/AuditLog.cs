using IdPPlatform.Domain.Common;

namespace IdPPlatform.Domain.Entities;

public class AuditLog : TenantEntity
{
    public Guid? UserId { get; private set; }
    public Guid? MembershipId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string ResourceType { get; private set; } = string.Empty;
    public Guid? ResourceId { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }

    private AuditLog()
    {
    }

    public AuditLog(
        Guid tenantId,
        Guid? userId,
        Guid? membershipId,
        string action,
        string resourceType,
        Guid? resourceId,
        string? ipAddress,
        string? userAgent) : base(tenantId)
    {
        UserId = userId;
        MembershipId = membershipId;
        Action = action;
        ResourceType = resourceType;
        ResourceId = resourceId;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }
}
