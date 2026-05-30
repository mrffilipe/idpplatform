namespace IdPPlatform.Client.Models;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record CreatedIdResponse(Guid Id);

public sealed record CreatedMembershipIdResponse(Guid MembershipId);
