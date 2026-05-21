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
}
