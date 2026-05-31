namespace Kyvo.Application.Interfaces;

public interface IPaged
{
    int Page { get; }

    int PageSize { get; }
}
