using IdPPlatform.Domain.Exceptions;

namespace IdPPlatform.Domain.ValueObjects;

public sealed record EmailAddress
{
    public string Value { get; private set; } = string.Empty;

    private EmailAddress()
    {
    }

    public EmailAddress(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainValidationException(DomainErrorMessages.EmailAddress.Required);
        }

        var normalized = email.Trim().ToLowerInvariant();
        if (normalized.Length > 255)
        {
            throw new DomainValidationException(DomainErrorMessages.EmailAddress.MaxLength);
        }

        if (!IsValidEmail(normalized))
        {
            throw new DomainValidationException(DomainErrorMessages.EmailAddress.InvalidFormat);
        }

        Value = normalized;
    }

    public static implicit operator string(EmailAddress value) => value.Value;
    public static implicit operator EmailAddress(string value) => new(value);

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
