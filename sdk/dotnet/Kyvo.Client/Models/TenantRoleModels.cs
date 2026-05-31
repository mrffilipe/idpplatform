namespace Kyvo.Client.Models;

public sealed record TenantRoleDto(
    Guid Id,
    string Key,
    string Name,
    bool IsActive);

public sealed record CreateTenantRoleBody(string Key, string Name);

public sealed record UpdateTenantRoleBody(string? Name, bool? IsActive);
