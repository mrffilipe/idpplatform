namespace PulseCrm.Api.Services;

public interface IIdPSubscribeClient
{
    Task<IdPTenantContextResult> SubscribeAsync(
        string accessToken,
        SubscribeTenantRequest request,
        CancellationToken cancellationToken = default);
}
