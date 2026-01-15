using Spendly.Domain.Exceptions;

namespace Spendly.Domain.Exceptions
{
    public class InvalidDomainException : DomainException 
    {
        public InvalidDomainException(string message) : base(message) { }
    }
}
