using Kyvo.Domain.Common;
using Kyvo.Domain.Exceptions;
using Kyvo.Domain.ValueObjects;

namespace Kyvo.Domain.Entities;

public class ExternalIdentity : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Provider { get; private set; } = string.Empty;
    public string ProviderUserId { get; private set; } = string.Empty;
    public EmailAddress Email { get; private set; } = null!;

    private ExternalIdentity()
    {
    }

    public ExternalIdentity(
        Guid userId,
        string provider,
        string providerUserId,
        string email)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainValidationException(DomainErrorMessages.ExternalIdentity.UserIdRequired);
        }

        if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(providerUserId))
        {
            throw new DomainValidationException(DomainErrorMessages.ExternalIdentity.ProviderDataRequired);
        }

        UserId = userId;
        Provider = provider.Trim().ToLowerInvariant();
        ProviderUserId = providerUserId.Trim();
        Email = new EmailAddress(email);
    }
}
