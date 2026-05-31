namespace Kyvo.AspNetCore;

/// <summary>
/// HTTP handler that optionally trusts any server certificate when calling a local Kyvo.
/// </summary>
public static class DevKyvoCertificateHandler
{
    public static HttpMessageHandler Create(bool allowInvalidCertificate) =>
        allowInvalidCertificate
            ? new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            }
            : new HttpClientHandler();
}
