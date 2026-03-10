namespace Spendly.Web.Contracts.RecurringExpenses
{
    public class RecurringExpenseDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? LastGeneratedDate { get; set; }
        public DateTime? NextOccurrence { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateRecurringExpenseDto
    {
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Category { get; set; } = string.Empty;
        public int Frequency { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Today;
        public DateTime? EndDate { get; set; }
    }

    public class UpdateRecurringExpenseDto
    {
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Category { get; set; } = string.Empty;
        public int Frequency { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class RecurringExpenseSummaryDto
    {
        public int TotalRecurrences { get; set; }
        public int ActiveRecurrences { get; set; }
        public decimal MonthlyProjectedTotal { get; set; }
        public List<RecurringExpenseDto> Recurrences { get; set; } = new();
    }
}