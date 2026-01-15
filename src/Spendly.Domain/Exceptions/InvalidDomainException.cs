using Spendly.Domain.Exceptions;

namespace Spendly.Domain.Exceptions
{
    public class InvalidDomainException : Exception
    {
        public InvalidDomainException(string message) : base(message) { }
    }
}
