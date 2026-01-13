namespace Spendly.Domain.Exceptions
{
    public class ExpenseNotFoundException : DomainException
    {
        public ExpenseNotFoundException(int id)
            : base($"Expense with id {id} was not found.")
        {
        }
    }
}
