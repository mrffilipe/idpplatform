using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PulseCrm.Api.Configuration;

namespace PulseCrm.Api.Services;

public sealed class IdPSubscribeClient : IIdPSubscribeClient
{
    private readonly HttpClient _httpClient;
    private readonly IdPOptions _options;

    public IdPSubscribeClient(HttpClient httpClient, IOptions<IdPOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<IdPTenantContextResult> SubscribeAsync(
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

        var result = JsonSerializer.Deserialize<IdPTenantContextResult>(
            body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (result is null)
        {
            throw new InvalidOperationException("IdP subscribe returned empty body.");
        }

        return result;
    }
}
