using TenancyKit.Abstractions;

namespace Kyvo.AspNetCore.TenancyKit;

/// <summary>
/// Default tenant info for product apps using claim passthrough (tid Guid).
/// </summary>
public class ProductTenantInfo : ITenantInfo
{
    public string Id { get; set; } = string.Empty;

    public string Identifier { get; set; } = string.Empty;
}
