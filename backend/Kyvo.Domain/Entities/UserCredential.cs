using Kyvo.Domain.Common;
using Kyvo.Domain.Exceptions;

namespace Kyvo.Domain.Entities;

public sealed class UserCredential : BaseEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public string PasswordHash { get; private set; } = string.Empty;

    private UserCredential()
    {
    }

    public UserCredential(Guid userId, string passwordHash)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainValidationException(DomainErrorMessages.UserCredential.UserIdRequired);
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainValidationException(DomainErrorMessages.UserCredential.PasswordHashRequired);
        }

        UserId = userId;
        PasswordHash = passwordHash;
    }

    public void UpdatePasswordHash(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
        {
            throw new DomainValidationException(DomainErrorMessages.UserCredential.PasswordHashRequired);
        }

        PasswordHash = newPasswordHash;
    }
}
