namespace IdPPlatform.Domain.Constants;

/// <summary>
/// Fixed constants for the platform admin console application.
/// These values are not configurable via API or panel; they are part of the product contract.
/// </summary>
public static class PlatformDefaults
{
    public static class AdminConsole
    {
        public const string ApplicationName = "Platform Admin";
        public const string ApplicationSlug = "platform-admin";
        public const string ClientId = "platform-admin-web";

        public static readonly IReadOnlyList<string> AllowedScopes =
            ["openid", "profile", "email", "offline_access"];

        public static readonly IReadOnlyList<string> DefaultRedirectUris =
            ["http://localhost:3000/auth/callback"];
    }

    public static class LocalIdentityProvider
    {
        public const string Alias = "local";
        public const string DisplayName = "Local";
    }
}
