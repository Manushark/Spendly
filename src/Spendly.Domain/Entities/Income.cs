using Spendly.Domain.Exceptions;
using Spendly.Domain.ValueObjects;

namespace Spendly.Domain.Entities
{
    /// <summary>
    /// Representa un ingreso del usuario.
    /// Permite calcular balance real (Ingresos - Gastos) y tasa de ahorro.
    /// </summary>
    public class Income
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }
        public Money Amount { get; private set; } = null!;
        public string Currency { get; private set; } = "USD";
        public string Source { get; private set; } = null!;
        public string? Description { get; private set; }
        public DateTime Date { get; private set; }
        public bool IsRecurring { get; private set; }
        public DateTime CreatedAt { get; private set; }

        protected Income() { }

        private Income(int userId, decimal amount, string currency, string source, string? description, DateTime date, bool isRecurring)
        {
            Validate(amount, source, date);
            UserId = userId;
            Amount = Money.Create(amount, currency);
            Currency = currency.ToUpperInvariant();
            Source = source;
            Description = description;
            Date = date;
            IsRecurring = isRecurring;
            CreatedAt = DateTime.UtcNow;
        }

        public void Update(decimal amount, string currency, string source, string? description, DateTime date, bool isRecurring)
        {
            Validate(amount, source, date);
            Amount = Money.Create(amount, currency);
            Currency = currency.ToUpperInvariant();
            Source = source;
            Description = description;
            Date = date;
            IsRecurring = isRecurring;
        }

        public void EnsureOwnership(int userId)
        {
            if (UserId != userId)
                throw new UnauthorizedAccessException($"Income {Id} does not belong to user {userId}.");
        }

        private static void Validate(decimal amount, string source, DateTime date)
        {
            if (amount <= 0)
                throw new InvalidDomainException("Amount must be greater than zero.");

            if (string.IsNullOrWhiteSpace(source))
                throw new InvalidDomainException("Source cannot be empty.");

            if (source.Length > 100)
                throw new InvalidDomainException("Source cannot exceed 100 characters.");

            // Allow a 14-hour buffer over UTC so users in any timezone are never
            // falsely rejected when submitting "today". The Application layer
            // enforces the real local-day boundary using the user's timezone.
            if (date.Date > DateTime.UtcNow.AddHours(14).Date)
                throw new InvalidDomainException("Date cannot be in the future.");
        }

        public static Income Create(int userId, decimal amount, string currency, string source, string? description, DateTime date, bool isRecurring = false)
            => new(userId, amount, currency, source, description, date, isRecurring);
    }
}
