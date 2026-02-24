using System.Globalization;

namespace Spendly.Application.DTOs.Dashboard
{
    /// <summary>
    /// Resumen completo del dashboard con todas las métricas.
    /// </summary>
    public class DashboardSummaryDto
    {
        public decimal TotalCurrentMonth { get; set; }
        public decimal TotalLastMonth { get; set; }
        public decimal PercentageChange { get; set; }
        public int ExpenseCountCurrentMonth { get; set; }
        public decimal AverageDailySpending { get; set; }
        public string TopCategory { get; set; } = string.Empty;
        public decimal TopCategoryAmount { get; set; }

        public List<CategorySpendingDto> SpendingByCategory { get; set; } = [];
        public List<MonthlyTrendDto> MonthlyTrend { get; set; } = [];
        public List<RecentExpenseDto> RecentExpenses { get; set; } = [];
    }

    /// <summary>
    /// Gasto por categoría (para gráfico de pie/dona).
    /// </summary>
    public class CategorySpendingDto
    {
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    /// <summary>
    /// Tendencia mensual (para gráfico de línea).
    /// </summary>
    public class MonthlyTrendDto
    {
        public string Month { get; set; } = string.Empty;  // "Jan 2026"
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// Gastos recientes para la tabla del dashboard.
    /// </summary>
    public class RecentExpenseDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Category { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int DaysAgo { get; set; }
    }
}