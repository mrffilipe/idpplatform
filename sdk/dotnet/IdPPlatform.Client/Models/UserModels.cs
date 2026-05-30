namespace IdPPlatform.Client.Models;

public sealed record UserDto(
    Guid Id,
    string Email,
    string? DisplayName,
    string? PhotoUrl,
    DateTime CreatedAt);

public sealed record UpdateUserProfileBody(string? DisplayName, string? PhotoUrl);

public sealed record UserMembershipDto(
    Guid MembershipId,
    Guid TenantId,
    string TenantName,
    string TenantKey,
    IReadOnlyList<string> Roles);
