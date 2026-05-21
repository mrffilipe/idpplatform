namespace IdPPlatform.Domain.Constants;

/// <summary>
/// Constantes fixas da aplicação admin console da plataforma.
/// Estes valores não são configuráveis por API nem por painel; são parte do contrato do produto.
/// </summary>
public static class PlatformDefaults
{
    public static class AdminConsole
    {
        public const string ApplicationName = "Platform Admin";
        public const string ApplicationSlug = "platform-admin";
        public const string ClientId = "platform-admin-web";

        public static readonly IReadOnlyList<string> AllowedScopes =
            ["openid", "profile", "email"];

        public static readonly IReadOnlyList<string> DefaultRedirectUris =
            ["http://localhost:3000/auth/callback"];
    }

    public static class LocalIdentityProvider
    {
        public const string Alias = "local";
        public const string DisplayName = "Local";
    }
}
