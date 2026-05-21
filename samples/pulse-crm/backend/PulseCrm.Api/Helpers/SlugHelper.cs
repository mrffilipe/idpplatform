using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace PulseCrm.Api.Helpers;

public static partial class SlugHelper
{
    public static string ToTenantKey(string companyName)
    {
        var normalized = companyName.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(ch);
            }
        }

        var slug = NonAlphanumeric().Replace(sb.ToString(), "-");
        slug = slug.Trim('-');

        while (slug.Contains("--", StringComparison.Ordinal))
        {
            slug = slug.Replace("--", "-", StringComparison.Ordinal);
        }

        if (slug.Length < 3)
        {
            slug = $"org-{Guid.NewGuid():N}"[..12];
        }

        return slug.Length > 60 ? slug[..60].TrimEnd('-') : slug;
    }

    [GeneratedRegex(@"[^a-z0-9\-]")]
    private static partial Regex NonAlphanumeric();
}
