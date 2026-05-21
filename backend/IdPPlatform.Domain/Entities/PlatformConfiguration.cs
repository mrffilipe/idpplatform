using IdPPlatform.Domain.Common;
using IdPPlatform.Domain.Exceptions;

namespace IdPPlatform.Domain.Entities;

public sealed class PlatformConfiguration : BaseEntity
{
    public bool IsBootstrapped { get; private set; }
    public Guid? RootUserId { get; private set; }
    public string? OauthClientId { get; private set; }
    public DateTime? BootstrappedAt { get; private set; }

    private PlatformConfiguration()
    {
    }

    public PlatformConfiguration(bool isBootstrapped = false)
    {
        IsBootstrapped = isBootstrapped;
    }

    public void MarkBootstrapped(Guid rootUserId, string oauthClientId)
    {
        if (IsBootstrapped)
        {
            throw new DomainBusinessRuleException(DomainErrorMessages.PlatformConfiguration.AlreadyBootstrapped);
        }

        if (rootUserId == Guid.Empty)
        {
            throw new DomainValidationException(DomainErrorMessages.PlatformConfiguration.RootUserIdRequired);
        }

        if (string.IsNullOrWhiteSpace(oauthClientId))
        {
            throw new DomainValidationException(DomainErrorMessages.PlatformConfiguration.OauthClientIdRequired);
        }

        IsBootstrapped = true;
        RootUserId = rootUserId;
        OauthClientId = oauthClientId.Trim();
        BootstrappedAt = DateTime.UtcNow;
    }
}
