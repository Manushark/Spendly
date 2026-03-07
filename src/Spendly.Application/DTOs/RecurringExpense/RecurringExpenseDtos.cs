using Spendly.Domain.Enums;

namespace Spendly.Application.DTOs.RecurringExpense
{
    public class RecurringExpenseResponseDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;  // "Daily", "Weekly", etc.
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
        public RecurrenceFrequency Frequency { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class UpdateRecurringExpenseDto
    {
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Category { get; set; } = string.Empty;
        public RecurrenceFrequency Frequency { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class RecurringExpenseSummaryDto
    {
        public int TotalRecurrences { get; set; }
        public int ActiveRecurrences { get; set; }
        public decimal MonthlyProjectedTotal { get; set; }
        public List<RecurringExpenseResponseDto> Recurrences { get; set; } = [];
    }
}
