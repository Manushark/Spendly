using Spendly.Domain.Exceptions;

namespace Spendly.Domain.Entities
{
    public class SavingsGoal
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }
        public string Name { get; private set; } = null!;
        public decimal TargetAmount { get; private set; }
        public decimal CurrentAmount { get; private set; }
        public DateTime? Deadline { get; private set; }
        public string Icon { get; private set; } = "bi-bullseye";
        public string Color { get; private set; } = "#6366f1";
        public bool IsCompleted { get; private set; }
        public DateTime CreatedAt { get; private set; }

        protected SavingsGoal() { }

        private SavingsGoal(int userId, string name, decimal targetAmount, decimal currentAmount, DateTime? deadline, string icon, string color)
        {
            Validate(name, targetAmount, currentAmount);
            UserId = userId;
            Name = name;
            TargetAmount = targetAmount;
            CurrentAmount = currentAmount;
            Deadline = deadline;
            Icon = string.IsNullOrWhiteSpace(icon) ? "bi-bullseye" : icon;
            Color = string.IsNullOrWhiteSpace(color) ? "#6366f1" : color;
            IsCompleted = currentAmount >= targetAmount;
            CreatedAt = DateTime.UtcNow;
        }

        public void Update(string name, decimal targetAmount, decimal currentAmount, DateTime? deadline, string icon, string color)
        {
            Validate(name, targetAmount, currentAmount);
            Name = name;
            TargetAmount = targetAmount;
            CurrentAmount = currentAmount;
            Deadline = deadline;
            Icon = string.IsNullOrWhiteSpace(icon) ? Icon : icon;
            Color = string.IsNullOrWhiteSpace(color) ? Color : color;
            IsCompleted = currentAmount >= targetAmount;
        }

        public void AddFunds(decimal amount)
        {
            if (amount <= 0)
                throw new InvalidDomainException("Amount to add must be positive.");

            CurrentAmount += amount;
            if (CurrentAmount >= TargetAmount)
                IsCompleted = true;
        }

        public void EnsureOwnership(int userId)
        {
            if (UserId != userId)
                throw new UnauthorizedAccessException($"You do not have access to savings goal {Id}.");
        }

        public decimal ProgressPercentage =>
            TargetAmount > 0 ? Math.Min(100, (CurrentAmount / TargetAmount) * 100) : 0;

        private static void Validate(string name, decimal targetAmount, decimal currentAmount)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidDomainException("Savings goal name cannot be empty.");

            if (name.Length > 100)
                throw new InvalidDomainException("Name cannot exceed 100 characters.");

            if (targetAmount <= 0)
                throw new InvalidDomainException("Target amount must be greater than zero.");

            if (currentAmount < 0)
                throw new InvalidDomainException("Current amount cannot be negative.");
        }

        public static SavingsGoal Create(int userId, string name, decimal targetAmount, decimal currentAmount, DateTime? deadline, string icon, string color)
            => new(userId, name, targetAmount, currentAmount, deadline, icon, color);
    }
}
