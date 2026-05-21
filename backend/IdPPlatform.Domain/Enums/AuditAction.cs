namespace IdPPlatform.Domain.Enums;

public enum AuditAction
{
    UserCreated = 0,
    UserUpdated = 1,
    TenantCreated = 2,
    TenantUpdated = 3,
    SessionCreated = 4,
    SessionRevoked = 5,
    MembershipCreated = 6,
    MembershipRevoked = 7,
    MembershipRoleUpdated = 8,
    InviteCreated = 9,
    InviteAccepted = 10
}
