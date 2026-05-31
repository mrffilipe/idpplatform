namespace Kyvo.Client.Models;

public sealed record MembershipDto(
    Guid Id,
    Guid UserId,
    string Email,
    string? DisplayName,
    IReadOnlyList<string> Roles,
    DateTime CreatedAt);

public sealed record CreateMembershipBody(Guid UserId, IReadOnlyList<string> RoleKeys);

public sealed record UpdateMembershipRolesBody(IReadOnlyList<string> RoleKeys);
