using Spendly.Domain.Exceptions;

namespace Spendly.Domain.Entities
{
    public class Budget
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }
        public string Category { get; private set; } = null!;
        public decimal MonthlyLimit { get; private set; }
        public int Year { get; private set; }
        public int Month { get; private set; }
        public DateTime CreatedAt { get; private set; }

        // Requerido por EF Core
        protected Budget() { }

        private Budget(int userId, string category, decimal monthlyLimit, int year, int month)
        {
            Validate(category, monthlyLimit, year, month);
            UserId = userId;
            Category = category;
            MonthlyLimit = monthlyLimit;
            Year = year;
            Month = month;
            CreatedAt = DateTime.UtcNow;
        }

        public void Update(string category, decimal monthlyLimit, int year, int month)
        {
            Validate(category, monthlyLimit, year, month);
            Category = category;
            MonthlyLimit = monthlyLimit;
            Year = year;
            Month = month;
        }

        public void EnsureOwnership(int userId)
        {
            if (UserId != userId)
                throw new UnauthorizedAccessException($"You do not have access to budget {Id}.");
        }

        private static void Validate(string category, decimal monthlyLimit, int year, int month)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new InvalidDomainException("Category cannot be empty.");

            if (category.Length > 100)
                throw new InvalidDomainException("Category cannot exceed 100 characters.");

            if (monthlyLimit <= 0)
                throw new InvalidDomainException("Monthly limit must be greater than zero.");

            if (year < 2000 || year > 2100)
                throw new InvalidDomainException("Year must be between 2000 and 2100.");

            if (month < 1 || month > 12)
                throw new InvalidDomainException("Month must be between 1 and 12.");
        }

        public static Budget Create(int userId, string category, decimal monthlyLimit, int year, int month)
            => new(userId, category, monthlyLimit, year, month);
    }
}