using System.Text.Json.Nodes;
using Kyvo.Application.Services.Security;
using Kyvo.Domain.Enums;

namespace Kyvo.Application.Services.IdentityProvider;

public sealed class IdentityProviderConfigCipher : IIdentityProviderConfigCipher
{
    // Top-level JSON paths inside ConfigJson that must be encrypted at rest, per provider type.
    // Only secrets are listed here: public identifiers (ProjectId, Issuer, JwksUri, Audience) remain in plain text.
    private static readonly IReadOnlyDictionary<IdentityProviderType, IReadOnlyList<string>> SensitivePathsByProvider =
        new Dictionary<IdentityProviderType, IReadOnlyList<string>>
        {
            [IdentityProviderType.Firebase] = ["WebApiKey", "ServiceAccount"],
            [IdentityProviderType.Cognito] = [],
            [IdentityProviderType.Generic] = [],
            [IdentityProviderType.Local] = []
        };

    private readonly ISecretProtector _protector;

    public IdentityProviderConfigCipher(ISecretProtector protector)
    {
        _protector = protector;
    }

    public string? Encrypt(IdentityProviderType providerType, string? configJson)
    {
        if (string.IsNullOrWhiteSpace(configJson))
        {
            return configJson;
        }

        if (!SensitivePathsByProvider.TryGetValue(providerType, out var paths) || paths.Count == 0)
        {
            return configJson;
        }

        if (JsonNode.Parse(configJson) is not JsonObject root)
        {
            return configJson;
        }

        foreach (var path in paths)
        {
            EncryptPath(root, path);
        }

        return root.ToJsonString();
    }

    public string? Decrypt(string? configJson)
    {
        if (string.IsNullOrWhiteSpace(configJson))
        {
            return configJson;
        }

        if (JsonNode.Parse(configJson) is not JsonObject root)
        {
            return configJson;
        }

        var mutated = false;
        foreach (var (key, value) in root.ToList())
        {
            if (value is not JsonValue jsonValue || !jsonValue.TryGetValue<string>(out var stringValue))
            {
                continue;
            }

            if (!_protector.IsProtected(stringValue))
            {
                continue;
            }

            var plaintextJson = _protector.Unprotect(stringValue);
            root[key] = JsonNode.Parse(plaintextJson);
            mutated = true;
        }

        return mutated ? root.ToJsonString() : configJson;
    }

    private void EncryptPath(JsonObject root, string path)
    {
        if (!root.TryGetPropertyValue(path, out var node) || node is null)
        {
            return;
        }

        // Skip values already encrypted (idempotent for already-protected payloads).
        if (node is JsonValue alreadyValue
            && alreadyValue.TryGetValue<string>(out var existing)
            && _protector.IsProtected(existing))
        {
            return;
        }

        // Serializing first guarantees the decrypted payload can be parsed back into the original JSON shape
        // (object, array, string, number, etc.).
        var plaintextJson = node.ToJsonString();
        var encrypted = _protector.Protect(plaintextJson);
        root[path] = JsonValue.Create(encrypted);
    }
}
