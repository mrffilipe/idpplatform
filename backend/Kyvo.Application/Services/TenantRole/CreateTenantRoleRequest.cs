namespace Kyvo.Application.Services.TenantRoles;

public sealed record CreateTenantRoleRequest
{
    public Guid TenantId { get; init; }

    public required string Key { get; init; }

    public required string Name { get; init; }

    public string? Description { get; init; }
}
