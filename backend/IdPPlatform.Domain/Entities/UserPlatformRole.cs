using IdPPlatform.Domain.Common;
using IdPPlatform.Domain.Exceptions;

namespace IdPPlatform.Domain.Entities;

public sealed class UserPlatformRole : BaseEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public Guid RoleId { get; private set; }
    public PlatformRole Role { get; private set; } = null!;

    private UserPlatformRole()
    {
    }

    public UserPlatformRole(Guid userId, Guid roleId)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainValidationException(DomainErrorMessages.UserPlatformRole.UserIdRequired);
        }

        if (roleId == Guid.Empty)
        {
            throw new DomainValidationException(DomainErrorMessages.UserPlatformRole.RoleIdRequired);
        }

        UserId = userId;
        RoleId = roleId;
    }
}
