namespace IdPPlatform.AspNetCore;

/// <summary>
/// HTTP handler that optionally trusts any server certificate when calling a local IdP.
/// </summary>
public static class DevIdpCertificateHandler
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
