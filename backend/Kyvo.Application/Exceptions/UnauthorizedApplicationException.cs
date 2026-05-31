namespace Kyvo.Application.Exceptions;

public sealed class UnauthorizedApplicationException : Exception
{
    public UnauthorizedApplicationException(string message) : base(message)
    {
    }
}
