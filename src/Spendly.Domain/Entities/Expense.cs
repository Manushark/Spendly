using Spendly.Domain.Exceptions;
using Spendly.Domain.ValueObjects;

namespace Spendly.Domain.Entities
{
    public class Expense
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }          // ← NUEVO: dueño del gasto
        public Money Amount { get; private set; }
        public string Description { get; private set; }
        public DateTime Date { get; private set; }
        public string Category { get; private set; }

        protected Expense() { }

        private Expense(int userId, Money amount, string description, DateTime date, string category)
        {
            UserId = userId;
            Validate(amount, description, date, category);
            Amount = amount;
            Description = description;
            Date = date;
            Category = category;
        }

        public void Update(Money amount, string description, DateTime date, string category)
        {
            Validate(amount, description, date, category);
            Amount = amount;
            Description = description;
            Date = date;
            Category = category;
        }

        /// <summary>
        /// Verifica que este gasto pertenezca al usuario indicado.
        /// </summary>
        public void EnsureOwnership(int userId)
        {
            if (UserId != userId)
                throw new UnauthorizedExpenseAccessException(Id);
        }

        private static void Validate(Money amount, string description, DateTime date, string category)
        {
            if (amount is null)
                throw new InvalidDomainException("Amount is required.");

            if (string.IsNullOrWhiteSpace(description))
                throw new InvalidDomainException("Description cannot be empty.");

            if (string.IsNullOrWhiteSpace(category))
                throw new InvalidDomainException("Category cannot be empty.");

            if (date > DateTime.UtcNow)
                throw new InvalidDomainException("Date cannot be in the future.");
        }

        public static Expense Create(int userId, Money amount, string description, DateTime date, string category)
        {
            return new Expense(userId, amount, description, date, category);
        }
    }
}