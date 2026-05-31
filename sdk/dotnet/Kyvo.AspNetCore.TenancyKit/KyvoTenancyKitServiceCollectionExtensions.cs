using TenancyKit.AspNetCore;
using TenancyKit.Abstractions;
using TenancyKit.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Kyvo.AspNetCore.TenancyKit;

public static class KyvoTenancyKitServiceCollectionExtensions
{
    public static IServiceCollection AddKyvoTenancyKit<TTenantInfo>(
        this IServiceCollection services,
        Action<TenancyKitOptions<TTenantInfo>> configure)
        where TTenantInfo : class, ITenantInfo, new()
    {
        ArgumentNullException.ThrowIfNull(configure);

        services.AddTenancyKit<TTenantInfo>(options =>
        {
            configure(options);
        });

        return services;
    }

    public static TenancyKitOptions<TTenantInfo> UseClaimPassthroughTenantStore<TTenantInfo>(
        this TenancyKitOptions<TTenantInfo> options)
        where TTenantInfo : ProductTenantInfo, new()
    {
        return options.UseStore(_ => new ClaimPassthroughTenantStore<TTenantInfo>());
    }
}
