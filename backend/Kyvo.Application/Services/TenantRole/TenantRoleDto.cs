namespace Kyvo.Application.Services.TenantRoles;

public sealed record TenantRoleDto
{
    public required Guid Id { get; init; }

    public required Guid TenantId { get; init; }

    public required string Key { get; init; }

    public required string Name { get; init; }

    public string? Description { get; init; }

    public required bool IsSystem { get; init; }

    public required bool IsActive { get; init; }
}
