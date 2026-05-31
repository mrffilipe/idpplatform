using System.Net;

namespace Kyvo.Client.Exceptions;

public sealed class KyvoApiException : Exception
{
    public KyvoApiException(
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
            return $"Kyvo API error ({(int)statusCode}): {detail}";
        }

        if (!string.IsNullOrWhiteSpace(title))
        {
            return $"Kyvo API error ({(int)statusCode}): {title}";
        }

        return $"Kyvo API error ({(int)statusCode}).";
    }
}
