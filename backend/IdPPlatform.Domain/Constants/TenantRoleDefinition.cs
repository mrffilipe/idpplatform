namespace IdPPlatform.Domain.Constants;

public sealed record TenantRoleDefinition
{
    public required string Key { get; init; }

    public required string Name { get; init; }
}
