using IdPPlatform.Domain.Exceptions;

namespace IdPPlatform.Domain.ValueObjects;

public sealed record PhotoUrl
{
    public const int MaxLength = 500;

    public string Value { get; private set; } = string.Empty;

    private PhotoUrl()
    {
    }

    public PhotoUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new DomainValidationException(DomainErrorMessages.PhotoUrl.Required);
        }

        var normalized = url.Trim();
        if (normalized.Length > MaxLength)
        {
            throw new DomainValidationException(DomainErrorMessages.PhotoUrl.MaxLength);
        }

        if (!IsValidAbsoluteHttpUrl(normalized))
        {
            throw new DomainValidationException(DomainErrorMessages.PhotoUrl.InvalidFormat);
        }

        Value = normalized;
    }

    public static PhotoUrl? FromNullable(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        return new PhotoUrl(url);
    }

    public static implicit operator string?(PhotoUrl? value) => value?.Value;

    public static implicit operator PhotoUrl?(string? value) => FromNullable(value);

    private static bool IsValidAbsoluteHttpUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
