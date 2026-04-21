namespace Spendly.Application.DTOs.Insights
{
    public class MonthlyInsightsDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;

        // Summary
        public decimal TotalExpenses { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal Balance { get; set; }
        public decimal SavingsRate { get; set; }

        // Comparison with previous month
        public decimal PreviousMonthExpenses { get; set; }
        public decimal ExpenseChangePercent { get; set; }
        public decimal PreviousMonthIncome { get; set; }
        public decimal IncomeChangePercent { get; set; }

        // Breakdown
        public List<CategoryInsightDto> CategoryBreakdown { get; set; } = [];
        public List<DailySpendingDto> DailySpending { get; set; } = [];
        public List<WeekdayInsightDto> WeekdayAnalysis { get; set; } = [];

        // Projection
        public decimal ProjectedMonthEnd { get; set; }
        public int DaysElapsed { get; set; }
        public int DaysInMonth { get; set; }
        public decimal DailyAverage { get; set; }
    }

    public class CategoryInsightDto
    {
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
        public int TransactionCount { get; set; }
        public decimal PreviousMonthAmount { get; set; }
        public decimal ChangePercent { get; set; }
    }

    public class DailySpendingDto
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
    }

    public class WeekdayInsightDto
    {
        public string DayName { get; set; } = string.Empty;
        public int DayOfWeek { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal Average { get; set; }
        public int TransactionCount { get; set; }
    }
}
