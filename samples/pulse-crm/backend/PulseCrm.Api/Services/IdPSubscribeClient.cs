using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PulseCrm.Api.Configuration;

namespace PulseCrm.Api.Services;

public sealed class IdPSubscribeClient : IIdPSubscribeClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly IdPOptions _options;

    public IdPSubscribeClient(HttpClient httpClient, IOptions<IdPOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<IdPSubscribeResult> SubscribeAsync(
        string accessToken,
        SubscribeTenantRequest request,
        CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, _options.SubscribePath);
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        message.Content = JsonContent.Create(request);

        var response = await _httpClient.SendAsync(message, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"IdP subscribe failed ({(int)response.StatusCode}): {body}");
        }

        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;

        var context = JsonSerializer.Deserialize<IdPTenantContextResult>(body, JsonOptions)
            ?? throw new InvalidOperationException("IdP subscribe returned empty body.");

        IdPOidcTokenPayload? tokens = null;
        if (root.TryGetProperty("tokens", out var tokensElement) && tokensElement.ValueKind == JsonValueKind.Object)
        {
            var access = tokensElement.GetProperty("access_token").GetString();
            if (!string.IsNullOrWhiteSpace(access))
            {
                tokens = new IdPOidcTokenPayload(
                    access,
                    tokensElement.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null,
                    tokensElement.TryGetProperty("expires_in", out var exp) ? exp.GetInt32() : 900,
                    tokensElement.TryGetProperty("token_type", out var tt) ? tt.GetString() ?? "Bearer" : "Bearer");
            }
        }

        return new IdPSubscribeResult(context, tokens);
    }
}
