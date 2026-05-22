using System.Text.Json;

namespace IdPPlatform.Application.IdentityProviderConfigs;

public sealed class FirebaseProviderConfig
{
    public string ProjectId { get; init; } = string.Empty;

    public string WebApiKey { get; init; } = string.Empty;

    /// <summary>
    /// Web app auth domain (e.g., my-project.firebaseapp.com). When empty, falls back to {projectId}.firebaseapp.com.
    /// </summary>
    public string? AuthDomain { get; init; }

    public JsonElement ServiceAccount { get; init; }

    public string ResolveAuthDomain() =>
        string.IsNullOrWhiteSpace(AuthDomain)
            ? $"{ProjectId.Trim()}.firebaseapp.com"
            : AuthDomain.Trim();
}
