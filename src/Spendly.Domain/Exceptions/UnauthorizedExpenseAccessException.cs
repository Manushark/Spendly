namespace Spendly.Domain.Exceptions
{
    public class UnauthorizedExpenseAccessException : DomainException
    {
        public UnauthorizedExpenseAccessException(int id)
            : base($"Access to expense {id} is not authorized.")
        {
        }
    }
}