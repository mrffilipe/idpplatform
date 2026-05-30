namespace PulseCrm.Api.Data;

public sealed class Contact : ITenantOwned
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public DateTime CreatedAt { get; set; }
}
