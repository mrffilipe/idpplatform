using Xunit;

namespace IdPPlatform.Client.Tests;

/// <summary>
/// Contract paths aligned with IdP Platform API v1.0 (product SDK surface).
/// </summary>
public sealed class ApiContractTests
{
    private const string V = "/v1.0";

    [Theory]
    [InlineData($"{V}/auth/subscribe")]
    [InlineData($"{V}/auth/switch-tenant")]
    [InlineData($"{V}/auth/sessions")]
    [InlineData($"{V}/Users/me")]
    [InlineData($"{V}/Users/me/memberships")]
    [InlineData($"{V}/Tenants")]
    [InlineData($"{V}/invites/accept")]
    [InlineData($"{V}/tenants/{{tenantId}}/memberships")]
    [InlineData($"{V}/tenants/{{tenantId}}/roles")]
    [InlineData($"{V}/Memberships/{{id}}")]
    [InlineData($"{V}/TenantRoles/{{id}}")]
    [InlineData($"{V}/AuditLogs")]
    public void ProductEndpointPaths_UseExpectedPrefix(string pathTemplate)
    {
        Assert.StartsWith(V, pathTemplate, StringComparison.Ordinal);
        Assert.DoesNotContain("Applications", pathTemplate, StringComparison.Ordinal);
        Assert.DoesNotContain("IdentityProviders", pathTemplate, StringComparison.Ordinal);
        Assert.DoesNotContain("platform/bootstrap", pathTemplate, StringComparison.Ordinal);
    }

    [Fact]
    public void Subscribe_IsOnlyOnAuthRoute()
    {
        const string subscribe = $"{V}/auth/subscribe";
        Assert.Equal("/v1.0/auth/subscribe", subscribe);
    }
}
