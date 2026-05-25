namespace PulseCrm.Api.Configuration;

/// <summary>
/// Allows the CRM API to call a local IdP served with a self-signed certificate (e.g. Docker on https://localhost:8443).
/// Only used in Development — never enable in production.
/// </summary>
internal static class DevIdpHttpHandler
{
    public static HttpMessageHandler Create(bool allowInvalidIdpCertificate) =>
        allowInvalidIdpCertificate
            ? new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            }
            : new HttpClientHandler();
}
