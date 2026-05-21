using IdPPlatform.Application.Common;

namespace IdPPlatform.Application.Services.AppService;

public interface IApplicationService
{
    Task<Guid> CreateAsync(CreateApplicationRequest request, CancellationToken cancellationToken = default);

    Task<Guid> CreateClientAsync(
        CreateApplicationClientRequest request,
        CancellationToken cancellationToken = default);

    Task<ProvisionApplicationTenantResult> ProvisionTenantAsync(
        ProvisionApplicationTenantRequest request,
        CancellationToken cancellationToken = default);

    Task<ApplicationDto?> GetByIdAsync(
        GetApplicationByIdRequest request,
        CancellationToken cancellationToken = default);

    Task<PagedResult<ApplicationDto>> ListAsync(
        ListApplicationsRequest request,
        CancellationToken cancellationToken = default);
}
