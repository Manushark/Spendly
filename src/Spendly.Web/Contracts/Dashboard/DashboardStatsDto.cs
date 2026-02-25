namespace Spendly.Web.Contracts.Dashboard
{
    public class DashboardStatsDto
    {
        public decimal CurrentMonthTotal { get; set; }
        public decimal PreviousMonthTotal { get; set; }
        public decimal DailyAverage { get; set; }
        public decimal HighestExpense { get; set; }
        public string TopCategory { get; set; } = string.Empty;
        public int TotalExpensesCount { get; set; }

        public List<CategoryStatsDto> CategoryBreakdown { get; set; } = [];
        public List<DailyTrendDto> DailyTrend { get; set; } = [];
        public List<TopExpenseDto> TopExpenses { get; set; } = [];

        // Propiedades calculadas para la vista
        public decimal MonthComparison => CurrentMonthTotal - PreviousMonthTotal;
        public decimal MonthComparisonPercentage => PreviousMonthTotal > 0
            ? ((CurrentMonthTotal - PreviousMonthTotal) / PreviousMonthTotal) * 100
            : 0;
        public bool IsIncreaseFromLastMonth => MonthComparison > 0;
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
        public string Date { get; set; } = string.Empty;
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
