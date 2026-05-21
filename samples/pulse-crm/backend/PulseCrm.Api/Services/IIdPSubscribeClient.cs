namespace PulseCrm.Api.Services;

public interface IIdPSubscribeClient
{
    Task<IdPSubscribeResult> SubscribeAsync(
        string accessToken,
        SubscribeTenantRequest request,
        CancellationToken cancellationToken = default);
}
