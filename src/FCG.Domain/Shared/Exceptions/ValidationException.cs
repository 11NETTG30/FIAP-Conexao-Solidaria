namespace FCG.Domain.Shared.Exceptions;

public class ValidationException : DomainException
{
    public ValidationException()
    {
     
    }

    public ValidationException(string message) : base(message)
    {
     
    }
 
    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
     
    }
}