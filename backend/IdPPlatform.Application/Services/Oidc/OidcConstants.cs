namespace IdPPlatform.Application.Services.Oidc;

public static class OidcConstants
{
    public static class Claims
    {
        public const string Subject = "sub";
        public const string Name = "name";
        public const string Email = "email";
    }

    public static class Scopes
    {
        public const string OpenId = "openid";
        public const string Profile = "profile";
        public const string Email = "email";
        public const string OfflineAccess = "offline_access";
    }

    public static class Errors
    {
        public const string InvalidRequest = "invalid_request";
        public const string InvalidClient = "invalid_client";
        public const string InvalidGrant = "invalid_grant";
        public const string UnauthorizedClient = "unauthorized_client";
        public const string UnsupportedGrantType = "unsupported_grant_type";
        public const string InvalidScope = "invalid_scope";
        public const string AccessDenied = "access_denied";
        public const string LoginRequired = "login_required";
    }

    public const string CodeChallengeMethodS256 = "S256";
    public const string ResourceName = "idpplatform-api";
}
