namespace Kyvo.Application.Services.TenantResolutionCache;

public interface ITenantResolutionCache
{
    Task InvalidateByIdentifierAsync(string identifier, CancellationToken cancellationToken = default);
}
