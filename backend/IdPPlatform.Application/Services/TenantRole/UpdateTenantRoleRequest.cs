namespace IdPPlatform.Application.Services.TenantRoles;

public sealed record UpdateTenantRoleRequest
{
    public required Guid RoleId { get; init; }

    public required string Name { get; init; }

    public string? Description { get; init; }

    public required bool IsActive { get; init; }
}
