namespace Spendly.Web.Contracts.Reports
{
    /// <summary>Reporte financiero mensual recibido desde la API.</summary>
    public class FinancialReportDto
    {
        public string PeriodLabel { get; set; } = string.Empty;
        public decimal TotalExpenses { get; set; }
        public decimal TotalIncomes { get; set; }
        public decimal NetBalance { get; set; }
        public decimal ExpenseToIncomeRatio { get; set; }
        public decimal AverageDailyExpense { get; set; }
        public string TopCategory { get; set; } = string.Empty;
        public decimal TopCategoryAmount { get; set; }
        public List<MonthlyFinancialTrendDto> MonthlyTrend { get; set; } = new();
        public List<CategoryReportItemDto> CategoryBreakdown { get; set; } = new();
    }

    public class MonthlyFinancialTrendDto
    {
        public string Month { get; set; } = string.Empty;
        public decimal Expenses { get; set; }
        public decimal Incomes { get; set; }
    }

    public class CategoryReportItemDto
    {
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
        public int TransactionCount { get; set; }
    }
}
