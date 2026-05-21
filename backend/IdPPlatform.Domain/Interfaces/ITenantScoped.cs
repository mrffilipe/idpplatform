namespace IdPPlatform.Domain.Interfaces;

public interface ITenantScoped
{
    Guid TenantId { get; }
}
