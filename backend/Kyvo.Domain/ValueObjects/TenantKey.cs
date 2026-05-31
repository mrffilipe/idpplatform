using System.Text.RegularExpressions;
using Kyvo.Domain.Exceptions;

namespace Kyvo.Domain.ValueObjects;

public sealed record TenantKey
{
    private static readonly Regex KeyRegex = new("^[a-z0-9][a-z0-9-]{1,62}$", RegexOptions.Compiled);

    public string Value { get; private set; } = string.Empty;

    private TenantKey()
    {
    }

    public TenantKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new DomainValidationException(DomainErrorMessages.TenantKey.Required);
        }

        var normalized = key.Trim().ToLowerInvariant();
        if (!KeyRegex.IsMatch(normalized))
        {
            throw new DomainValidationException(DomainErrorMessages.TenantKey.InvalidFormat);
        }

        Value = normalized;
    }

    public static implicit operator string(TenantKey value) => value.Value;
    public static implicit operator TenantKey(string value) => new(value);
}
