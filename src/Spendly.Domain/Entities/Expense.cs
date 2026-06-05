using Spendly.Domain.Exceptions;
using Spendly.Domain.ValueObjects;

namespace Spendly.Domain.Entities
{
    public class Expense
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }          // ← dueño del gasto
        public Money Amount { get; private set; }
        public string Currency { get; private set; } = "USD";
        public string Description { get; private set; }
        public DateTime Date { get; private set; }
        public string Category { get; private set; }
        
        // Soft delete properties
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAt { get; private set; }

        protected Expense() { }

        private Expense(int userId, Money amount, string description, DateTime date, string category)
        {
            UserId = userId;
            Validate(amount, description, date, category);
            Amount = amount;
            Currency = amount.Currency;
            Description = description;
            Date = date;
            Category = category;
        }

        public void Update(Money amount, string description, DateTime date, string category)
        {
            Validate(amount, description, date, category);
            Amount = amount;
            Currency = amount.Currency;
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

        public void Delete()
        {
            if (IsDeleted) return;
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
        }

        private static void Validate(Money amount, string description, DateTime date, string category)
        {
            if (amount is null)
                throw new InvalidDomainException("Amount is required.");

            if (string.IsNullOrWhiteSpace(description))
                throw new InvalidDomainException("Description cannot be empty.");

            if (string.IsNullOrWhiteSpace(category))
                throw new InvalidDomainException("Category cannot be empty.");

            // Allow a 14-hour buffer over UTC so users in UTC+14 (max world offset)
            // or UTC-12 (min) are never falsely rejected when submitting "today".
            // The Application layer enforces the real local-day boundary using the user's timezone.
            if (date.Date > DateTime.UtcNow.AddHours(14).Date)
                throw new InvalidDomainException("Date cannot be in the future.");
        }

        public static Expense Create(int userId, Money amount, string description, DateTime date, string category)
        {
            return new Expense(userId, amount, description, date, category);
        }
    }
}