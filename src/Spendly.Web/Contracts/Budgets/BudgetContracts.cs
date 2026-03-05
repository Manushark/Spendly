namespace Spendly.Web.Contracts.Budgets
{
    public class BudgetDto
    {
        public int Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public decimal MonthlyLimit { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Spent { get; set; }
        public decimal Remaining { get; set; }
        public decimal PercentageUsed { get; set; }
        public bool IsOverBudget { get; set; }
    }

    public class CreateBudgetDto
    {
        public string Category { get; set; } = string.Empty;
        public decimal MonthlyLimit { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
    }

    public class UpdateBudgetDto : CreateBudgetDto { }

    public class BudgetSummaryDto
    {
        public int TotalBudgets { get; set; }
        public decimal TotalLimit { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal TotalRemaining { get; set; }
        public int BudgetsExceeded { get; set; }
        public List<BudgetDto> Budgets { get; set; } = [];
    }
}