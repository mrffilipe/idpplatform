namespace IdPPlatform.Application.Exceptions;

public static class ApplicationErrorMessages
{
    public static class ExternalIdentity
    {
        public const string InvalidToken = "Invalid external identity token.";
        public const string EmailMissing = "External identity token does not include email.";
    }

    public static class OAuthClient
    {
        public const string ClientIdRequired = "client_id is required.";
        public const string ClientIdInvalid = "client_id is invalid.";
        public const string ClientSecretRequired = "client_secret is required for confidential clients.";
        public const string ClientSecretInvalid = "client_secret is invalid.";
        public const string RedirectUriNotAllowed = "redirect_uri is not allowed for this client.";
        public const string RedirectUrisRequired = "At least one redirect URI is required.";
        public const string AllowedScopesRequired = "At least one allowed scope is required.";
        public const string ConfigurationInvalid = "Client configuration is invalid.";
        public const string RequestedScopesNotAllowed = "Requested scopes are not allowed: {0}.";
    }

    public static class Pkce
    {
        public const string CodeChallengeRequired = "Public clients must send code_challenge.";
        public const string CodeChallengeLength = "code_challenge must be between 43 and 128 characters.";
        public const string CodeChallengeMethodUnsupported = "Unsupported code_challenge_method.";
    }

    public static class Auth
    {
        public const string UserHasNoClientTenantMembership = "User has no active membership for this client tenant.";
        public const string PlatformBootstrapRequired = "Platform bootstrap is required before login.";
        public const string PlatformBootstrapAlreadyCompleted = "Platform bootstrap has already been completed.";
        public const string PlatformBootstrapAdminCredentialsNotConfigured = "Bootstrap admin credentials are not configured. Set Bootstrap__AdminEmail and Bootstrap__AdminPassword environment variables (or Bootstrap:AdminEmail / Bootstrap:AdminPassword in appsettings).";
        public const string PlatformBootstrapApplicationSlugAlreadyExists = "Bootstrap application slug already exists.";
        public const string PlatformBootstrapClientIdAlreadyExists = "Bootstrap OAuth client id already exists.";
        public const string SessionInactive = "Session is not active.";
        public const string AuthenticatedSessionRequired = "Authenticated session is required.";
        public const string SessionHasNoOAuthClient = "Current session has no OAuth client context.";
        public const string UserHasNoTenantAccess = "User has no access to this tenant.";
        public const string SessionNotFound = "Session not found.";
        public const string UserNotFound = "User not found.";
        public const string CannotRevokeAnotherUserSession = "You do not have permission to revoke this session.";
        public const string LocalAuthInvalidCredentials = "Invalid email or password.";
    }

    public static class Application
    {
        public const string NotFound = "Application not found.";
        public const string SystemApplicationCannotBeModified = "System applications cannot be modified.";
    }

    public static class IdentityProvider
    {
        public const string NotFound = "Identity provider not found.";
        public const string AliasAlreadyExists = "Identity provider alias already exists.";
        public const string CannotDisableLastLocalProvider = "Cannot disable the only active local identity provider.";
        public const string Disabled = "Identity provider is disabled.";
        public const string LocalNotAllowedForExternalLogin = "Local identity provider cannot be used for external login.";
        public const string ConfigInvalid = "Identity provider configuration is invalid.";
        public const string ConfigRequired = "Identity provider configuration (ConfigJson) is required for this provider type.";
        public const string LoginTypeNotSupported = "Login is not yet supported for this identity provider type.";
    }

    public static class Signing
    {
        public const string RsaKeyNotAvailable = "RSA key is not available.";
    }

    public static class Registration
    {
        public const string EmailRequired = "Email is required.";
        public const string EmailAlreadyExists = "An account with this email already exists.";
        public const string PasswordRequired = "Password is required.";
        public const string PasswordTooWeak = "Password does not meet the minimum policy.";
        public const string DisplayNameRequired = "Display name is required.";
        public const string LocalPasswordDisabled = "Self-registration is disabled because no local identity provider is enabled.";
    }

    public static class IdentityProviderCapability
    {
        public const string LocalPasswordAlreadyHandled = "Another active identity provider already handles email/password authentication.";
        public const string SocialAlreadyHandledFormat = "Another enabled provider already advertises the {0} capability: {1}.";
    }
}
