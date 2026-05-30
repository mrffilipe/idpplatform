namespace PulseCrm.Api.Data;

public interface ITenantOwned
{
    Guid TenantId { get; set; }
}
