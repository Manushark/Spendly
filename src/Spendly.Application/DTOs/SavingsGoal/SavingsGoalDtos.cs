namespace Spendly.Application.DTOs.SavingsGoal
{
    public class CreateSavingsGoalDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public DateTime? Deadline { get; set; }
        public string Icon { get; set; } = "bi-bullseye";
        public string Color { get; set; } = "#6366f1";
    }

    public class UpdateSavingsGoalDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public DateTime? Deadline { get; set; }
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }

    public class AddFundsDto
    {
        public decimal Amount { get; set; }
    }

    public class SavingsGoalResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public decimal ProgressPercentage { get; set; }
        public DateTime? Deadline { get; set; }
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
