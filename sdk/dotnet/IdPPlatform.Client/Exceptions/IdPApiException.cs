using System.Net;

namespace IdPPlatform.Client.Exceptions;

public sealed class IdPApiException : Exception
{
    public IdPApiException(
        HttpStatusCode statusCode,
        string? title,
        string? detail,
        string? rawBody,
        Exception? innerException = null)
        : base(FormatMessage(statusCode, title, detail), innerException)
    {
        StatusCode = statusCode;
        Title = title;
        Detail = detail;
        RawBody = rawBody;
    }

    public HttpStatusCode StatusCode { get; }

    public string? Title { get; }

    public string? Detail { get; }

    public string? RawBody { get; }

    private static string FormatMessage(HttpStatusCode statusCode, string? title, string? detail)
    {
        if (!string.IsNullOrWhiteSpace(detail))
        {
            return $"IdP API error ({(int)statusCode}): {detail}";
        }

        if (!string.IsNullOrWhiteSpace(title))
        {
            return $"IdP API error ({(int)statusCode}): {title}";
        }

        return $"IdP API error ({(int)statusCode}).";
    }
}
