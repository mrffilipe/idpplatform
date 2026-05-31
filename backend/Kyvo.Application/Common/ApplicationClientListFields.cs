using System.Text.Json;
using Kyvo.Application.Exceptions;
using Kyvo.Domain.Exceptions;

namespace Kyvo.Application.Common;

public static class ApplicationClientListFields
{
    public static string ToRedirectUrisJson(string raw) => ToJsonArray(raw, ApplicationErrorMessages.OAuthClient.RedirectUrisRequired);

    public static string ToAllowedScopesJson(string raw) => ToJsonArray(raw, ApplicationErrorMessages.OAuthClient.AllowedScopesRequired);

    private static string ToJsonArray(string raw, string emptyMessage)
    {
        var values = Parse(raw);
        if (values.Count == 0)
        {
            throw new DomainValidationException(emptyMessage);
        }

        return JsonSerializer.Serialize(values);
    }

    private static IReadOnlyList<string> Parse(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return [];
        }

        var trimmed = raw.Trim();
        if (trimmed.StartsWith('['))
        {
            try
            {
                var jsonValues = JsonSerializer.Deserialize<string[]>(trimmed) ?? [];
                return Normalize(jsonValues);
            }
            catch (JsonException)
            {
                throw new DomainValidationException(ApplicationErrorMessages.OAuthClient.ConfigurationInvalid);
            }
        }

        if (trimmed.Contains(',') || trimmed.Contains('\n') || trimmed.Contains('\r') || trimmed.Contains(';'))
        {
            return Normalize(trimmed.Split([',', '\n', '\r', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        }

        return Normalize(trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    private static IReadOnlyList<string> Normalize(IEnumerable<string> values)
    {
        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
