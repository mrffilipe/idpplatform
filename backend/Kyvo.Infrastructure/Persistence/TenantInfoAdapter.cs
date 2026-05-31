using Kyvo.Domain.Entities;
using TenancyKit.Abstractions;

namespace Kyvo.Infrastructure.Persistence;

public sealed class TenantInfoAdapter : ITenantInfo
{
    public TenantInfoAdapter(Tenant tenant)
    {
        Id = tenant.Id.ToString("D");
        Identifier = tenant.Key;
        Name = tenant.Name;
    }

    public TenantInfoAdapter(string id, string identifier, string name)
    {
        Id = id;
        Identifier = identifier;
        Name = name;
    }

    public string Id { get; }
    public string Identifier { get; }
    public string Name { get; }
}
