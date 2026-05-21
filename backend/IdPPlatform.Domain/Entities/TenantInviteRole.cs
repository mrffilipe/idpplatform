using IdPPlatform.Domain.Common;
using IdPPlatform.Domain.Exceptions;

namespace IdPPlatform.Domain.Entities;

public sealed class TenantInviteRole : TenantEntity
{
    public Guid InviteId { get; private set; }
    public TenantInvite Invite { get; private set; } = null!;

    public Guid RoleId { get; private set; }
    public TenantRole Role { get; private set; } = null!;

    private TenantInviteRole()
    {
    }

    public TenantInviteRole(
        Guid tenantId,
        Guid inviteId,
        TenantRole role) : base(tenantId)
    {
        if (inviteId == Guid.Empty)
        {
            throw new DomainValidationException(DomainErrorMessages.TenantInvite.InviteNotFound);
        }

        if (role.TenantId != tenantId)
        {
            throw new DomainValidationException(DomainErrorMessages.TenantRole.RoleTenantMismatch);
        }

        InviteId = inviteId;
        RoleId = role.Id;
        Role = role;
    }
}
