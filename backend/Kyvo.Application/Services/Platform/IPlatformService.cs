namespace Kyvo.Application.Services.Platform;

public interface IPlatformService
{
    Task<BootstrapResult> BootstrapAsync(BootstrapRequest request, CancellationToken cancellationToken = default);

    Task<PlatformStatusResult> GetStatusAsync(CancellationToken cancellationToken = default);
}
