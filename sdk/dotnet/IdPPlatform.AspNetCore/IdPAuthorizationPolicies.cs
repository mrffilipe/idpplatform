using Microsoft.AspNetCore.Authorization;

namespace IdPPlatform.AspNetCore;

public static class IdPAuthorizationPolicies
{
    public const string RequireTenant = "IdP.RequireTenant";

    public const string RequireTenantAdmin = "IdP.RequireTenantAdmin";

    public static void AddIdPPlatformPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(RequireTenant, policy =>
            policy.RequireAssertion(ctx => HasClaim(ctx.User, "tid")));

        options.AddPolicy(RequireTenantAdmin, policy =>
            policy.RequireAssertion(ctx =>
                HasClaim(ctx.User, "tid")
                && ctx.User.FindAll("trole").Any(c =>
                    string.Equals(c.Value, "owner", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(c.Value, "admin", StringComparison.OrdinalIgnoreCase))));
    }

    public static AuthorizationPolicyBuilder RequireTenantRole(
        this AuthorizationPolicyBuilder builder,
        params string[] roles)
    {
        return builder.RequireAssertion(ctx =>
        {
            if (!HasClaim(ctx.User, "tid"))
            {
                return false;
            }

            var tenantRoles = ctx.User.FindAll("trole").Select(c => c.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
            return roles.Any(r => tenantRoles.Contains(r));
        });
    }

    private static bool HasClaim(System.Security.Claims.ClaimsPrincipal user, string claimType) =>
        user.HasClaim(c => c.Type == claimType && !string.IsNullOrWhiteSpace(c.Value));
}
