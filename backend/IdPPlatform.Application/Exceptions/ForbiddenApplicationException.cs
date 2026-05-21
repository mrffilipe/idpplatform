namespace IdPPlatform.Application.Exceptions;

public sealed class ForbiddenApplicationException : Exception
{
    public ForbiddenApplicationException(string message) : base(message)
    {
    }
}
