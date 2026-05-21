using IdPPlatform.Application.Interfaces;

namespace IdPPlatform.Application.Common;

public sealed record PagedResult<T> : IPaged
{
    public required IReadOnlyList<T> Items { get; init; }

    public int Total { get; init; }

    public int Page { get; init; }

    public int PageSize { get; init; }
}
