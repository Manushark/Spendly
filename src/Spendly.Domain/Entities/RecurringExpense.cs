using Spendly.Domain.Enums;
using Spendly.Domain.Exceptions;
using Spendly.Domain.ValueObjects;

namespace Spendly.Domain.Entities
{
    /// <summary>
    /// Plantilla de gasto recurrente.
    /// Representa un gasto que se repite automáticamente con cierta frecuencia.
    /// </summary>
    public class RecurringExpense
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }
        public string Description { get; private set; } = null!;
        public Money Amount { get; private set; } = null!;
        public string Category { get; private set; } = null!;
        public RecurrenceFrequency Frequency { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }  // null = indefinido
        public DateTime? LastGeneratedDate { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }

        protected RecurringExpense() { }

        private RecurringExpense(
            int userId,
            string description,
            decimal amount,
            string category,
            RecurrenceFrequency frequency,
            DateTime startDate,
            DateTime? endDate)
        {
            Validate(description, amount, category, startDate, endDate);

            UserId = userId;
            Description = description;
            Amount = Money.FromDecimal(amount);
            Category = category;
            Frequency = frequency;
            StartDate = startDate.Date;
            EndDate = endDate?.Date;
            LastGeneratedDate = null;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        public void Update(
            string description,
            decimal amount,
            string category,
            RecurrenceFrequency frequency,
            DateTime startDate,
            DateTime? endDate)
        {
            Validate(description, amount, category, startDate, endDate);

            Description = description;
            Amount = Money.FromDecimal(amount);
            Category = category;
            Frequency = frequency;
            StartDate = startDate.Date;
            EndDate = endDate?.Date;
        }

        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;

        public void MarkAsGenerated(DateTime generatedDate)
        {
            LastGeneratedDate = generatedDate.Date;
        }

        public void EnsureOwnership(int userId)
        {
            if (UserId != userId)
                throw new UnauthorizedAccessException(
                    $"Recurring expense {Id} does not belong to user {userId}.");
        }

        /// <summary>
        /// Calcula la próxima fecha en que debe generarse un gasto.
        /// </summary>
        public DateTime? GetNextOccurrence()
        {
            if (!IsActive) return null;

            var referenceDate = LastGeneratedDate ?? StartDate;
            var nextDate = Frequency switch
            {
                RecurrenceFrequency.Daily => referenceDate.AddDays(1),
                RecurrenceFrequency.Weekly => referenceDate.AddDays(7),
                RecurrenceFrequency.Monthly => referenceDate.AddMonths(1),
                RecurrenceFrequency.Yearly => referenceDate.AddYears(1),
                _ => throw new InvalidOperationException($"Unknown frequency: {Frequency}")
            };

            // Si hay fecha de fin y ya pasó, retornar null
            if (EndDate.HasValue && nextDate > EndDate.Value)
                return null;

            return nextDate;
        }

        /// <summary>
        /// Verifica si debe generar un gasto hoy.
        /// </summary>
        public bool ShouldGenerateToday()
        {
            if (!IsActive) return false;

            var today = DateTime.Today;
            var nextOccurrence = GetNextOccurrence();

            return nextOccurrence.HasValue && nextOccurrence.Value <= today;
        }

        private static void Validate(
            string description,
            decimal amount,
            string category,
            DateTime startDate,
            DateTime? endDate)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new InvalidDomainException("Description cannot be empty.");

            if (description.Length > 200)
                throw new InvalidDomainException("Description cannot exceed 200 characters.");

            if (amount <= 0)
                throw new InvalidDomainException("Amount must be greater than zero.");

            if (string.IsNullOrWhiteSpace(category))
                throw new InvalidDomainException("Category cannot be empty.");

            if (category.Length > 100)
                throw new InvalidDomainException("Category cannot exceed 100 characters.");

            if (endDate.HasValue && endDate.Value < startDate)
                throw new InvalidDomainException("End date cannot be before start date.");
        }

        public static RecurringExpense Create(
            int userId,
            string description,
            decimal amount,
            string category,
            RecurrenceFrequency frequency,
            DateTime startDate,
            DateTime? endDate = null)
            => new(userId, description, amount, category, frequency, startDate, endDate);
    }
}
