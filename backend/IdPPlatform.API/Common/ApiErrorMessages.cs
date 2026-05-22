namespace IdPPlatform.API.Common;

public static class ApiErrorMessages
{
    public const string UnauthorizedTitle = "Unauthorized";
    public const string ForbiddenTitle = "Forbidden";
    public const string DomainValidationTitle = "Domain Validation Error";
    public const string InvalidClientTitle = "Invalid Client";
    public const string DomainBusinessRuleTitle = "Domain Business Rule Error";
    public const string NotFoundTitle = "Not Found";
    public const string UnhandledServerErrorTitle = "Unhandled Server Error";
    public const string UnexpectedErrorDetail = "Unexpected error while processing the request.";
    public const string ProblemJsonContentType = "application/problem+json";

    public static class OidcLogin
    {
        public const string MissingLoginContext = "Login context is missing from the authentication cookie.";
        public const string InvalidLoginContext = "Login context is invalid.";
        public const string InteractiveLoginRequired = "Interactive login is required.";
        public const string SessionNoLongerActive = "Session is no longer active.";
        public const string UnableToBuildClaims = "Unable to build token claims.";
    }

    public static class Account
    {
        public const string EmailAndPasswordRequired = "Email and password are required.";
        public const string InvalidEmailOrPassword = "Invalid email or password.";
        public const string InvalidProviderOrToken = "Invalid provider or token.";
    }
}
