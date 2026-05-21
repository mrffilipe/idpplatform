namespace IdPPlatform.Domain.Exceptions;

public sealed class InvalidClientException : DomainException
{
    public InvalidClientException(string message) : base(message)
    {
    }
}
