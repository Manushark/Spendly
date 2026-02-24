namespace Spendly.Application.DTOs.Dashboard
{
    public class DashboardStatsDto
    {
        public decimal CurrentMonthTotal { get; set; }
        public decimal PreviousMonthTotal { get; set; }
        public decimal DailyAverage { get; set; }
        public decimal HighestExpense { get; set; }
        public string TopCategory { get; set; } = string.Empty;
        public int TotalExpensesCount { get; set; }
        
        public IEnumerable<CategoryStatsDto> CategoryBreakdown { get; set; } = [];
        public IEnumerable<DailyTrendDto> DailyTrend { get; set; } = [];
        public IEnumerable<TopExpenseDto> TopExpenses { get; set; } = [];
    }

    public class CategoryStatsDto
    {
        public string Category { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class DailyTrendDto
    {
        public string Date { get; set; } = string.Empty;  // formato: "Jan 15"
        public decimal Amount { get; set; }
    }

    public class TopExpenseDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Category { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
}