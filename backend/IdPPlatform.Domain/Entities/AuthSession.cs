using IdPPlatform.Domain.Common;
using IdPPlatform.Domain.Enums;
using IdPPlatform.Domain.Exceptions;

namespace IdPPlatform.Domain.Entities;

public class AuthSession : BaseEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public Guid? ClientId { get; private set; }
    public Guid? TenantId { get; private set; }
    public Guid? MembershipId { get; private set; }
    public SessionStatus Status { get; private set; }
    public string? UserAgent { get; private set; }
    public string? IpAddress { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime LastActivityAt { get; private set; }

    private AuthSession()
    {
    }

    public AuthSession(
        Guid userId,
        Guid? clientId,
        Guid? tenantId,
        Guid? membershipId,
        DateTime expiresAt,
        string? userAgent,
        string? ipAddress)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainValidationException(DomainErrorMessages.AuthSession.UserIdRequired);
        }

        UserId = userId;
        ClientId = clientId;
        TenantId = tenantId;
        MembershipId = membershipId;
        Status = SessionStatus.Active;
        UserAgent = userAgent?.Trim();
        IpAddress = ipAddress?.Trim();
        ExpiresAt = expiresAt;
        LastActivityAt = DateTime.UtcNow;
    }

    public void Touch() => LastActivityAt = DateTime.UtcNow;

    public void SwitchTenant(Guid tenantId, Guid membershipId)
    {
        if (tenantId == Guid.Empty || membershipId == Guid.Empty)
        {
            throw new DomainValidationException(DomainErrorMessages.AuthSession.TenantContextInvalid);
        }

        TenantId = tenantId;
        MembershipId = membershipId;
        Touch();
    }

    public void BindOAuthClient(Guid applicationClientId)
    {
        if (applicationClientId == Guid.Empty)
        {
            throw new DomainValidationException(DomainErrorMessages.AuthSession.ClientIdRequired);
        }

        ClientId = applicationClientId;
        Touch();
    }

    public void Revoke() => Status = SessionStatus.Revoked;
}
