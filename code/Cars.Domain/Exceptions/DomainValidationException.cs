namespace Cars.Domain.Exceptions;

public class DomainValidationException : DomainException
{
    public DomainValidationException(string message) : base(message) { }
    public DomainValidationException() : base("Domain-level validation error") { }
}