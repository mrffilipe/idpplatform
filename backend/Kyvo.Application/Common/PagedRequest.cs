using Kyvo.Application.Interfaces;

namespace Kyvo.Application.Common;

public abstract record PagedRequest : IPaged
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}
