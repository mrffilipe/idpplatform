using System.Net;
using System.Text.Json;
using Kyvo.Client.Exceptions;

namespace Kyvo.Client.Internal;

internal static class KyvoHttpResponse
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        string? title = null;
        string? detail = null;

        if (response.Content.Headers.ContentType?.MediaType?.Contains("problem+json", StringComparison.OrdinalIgnoreCase) == true
            || body.TrimStart().StartsWith('{'))
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                if (root.TryGetProperty("title", out var t)) title = t.GetString();
                if (root.TryGetProperty("detail", out var d)) detail = d.GetString();
            }
            catch (JsonException)
            {
                // ignore parse errors
            }
        }

        throw new KyvoApiException(response.StatusCode, title, detail ?? body, body);
    }

    public static async Task<T?> ReadJsonAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await EnsureSuccessAsync(response, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return default;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken);
    }

    public static JsonSerializerOptions SerializerOptions => JsonOptions;
}
