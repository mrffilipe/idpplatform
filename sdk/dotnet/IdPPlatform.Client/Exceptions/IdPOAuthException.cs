namespace IdPPlatform.Client.Exceptions;

public sealed class IdPOAuthException : Exception
{
    public IdPOAuthException(string error, string? errorDescription, string? rawBody)
        : base(FormatMessage(error, errorDescription))
    {
        Error = error;
        ErrorDescription = errorDescription;
        RawBody = rawBody;
    }

    public string Error { get; }

    public string? ErrorDescription { get; }

    public string? RawBody { get; }

    private static string FormatMessage(string error, string? description) =>
        string.IsNullOrWhiteSpace(description)
            ? $"OAuth error: {error}"
            : $"OAuth error: {error} — {description}";
}
