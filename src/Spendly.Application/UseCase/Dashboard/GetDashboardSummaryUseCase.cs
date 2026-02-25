using Spendly.Application.DTOs.Dashboard;
using Spendly.Application.Interfaces;

namespace Spendly.Application.UseCases.Dashboard
{
    public class GetDashboardSummaryUseCase
    {
        private readonly IExpenseRepository _repo;

        public GetDashboardSummaryUseCase(IExpenseRepository repo) => _repo = repo;

        public DashboardSummaryDto Execute(int userId)
        {
            var now = DateTime.UtcNow;
            var currentMonthStart = new DateTime(now.Year, now.Month, 1);
            var currentMonthEnd = currentMonthStart.AddMonths(1).AddDays(-1);
            var lastMonthStart = currentMonthStart.AddMonths(-1);
            var lastMonthEnd = currentMonthStart.AddDays(-1);

            // Totales del mes actual y anterior
            var totalCurrentMonth = _repo.GetTotalAmount(userId, currentMonthStart, currentMonthEnd);
            var totalLastMonth = _repo.GetTotalAmount(userId, lastMonthStart, lastMonthEnd);

            // Porcentaje de cambio
            var percentageChange = totalLastMonth > 0
                ? ((totalCurrentMonth - totalLastMonth) / totalLastMonth) * 100
                : 0;

            // Gastos por categoría (mes actual)
            var categoryTotals = _repo.GetTotalByCategory(userId, currentMonthStart, currentMonthEnd);
            var expensesCurrentMonth = _repo.GetByDateRange(userId, currentMonthStart, currentMonthEnd).ToList();

            var spendingByCategory = categoryTotals
                .Select(kvp => new CategorySpendingDto
                {
                    Category = kvp.Key,
                    Amount = kvp.Value,
                    Count = expensesCurrentMonth.Count(e => e.Category == kvp.Key),
                    Percentage = totalCurrentMonth > 0 ? (kvp.Value / totalCurrentMonth) * 100 : 0
                })
                .OrderByDescending(c => c.Amount)
                .ToList();

            // Top categoría
            var topCategory = spendingByCategory.FirstOrDefault();

            // Tendencia mensual (últimos 6 meses)
            var monthlyTotals = _repo.GetMonthlyTotals(userId, 6);
            var monthlyTrend = monthlyTotals
                .OrderBy(kvp => kvp.Key)
                .Select(kvp =>
                {
                    var expenses = _repo.GetByDateRange(userId, kvp.Key, kvp.Key.AddMonths(1).AddDays(-1));
                    return new MonthlyTrendDto
                    {
                        Month = kvp.Key.ToString("MMM yyyy"),
                        Amount = kvp.Value,
                        Count = expenses.Count()
                    };
                })
                .ToList();

            // Gastos recientes (últimos 5)
            var recentExpenses = _repo.GetRecent(userId, 5)
                .Select(e => new RecentExpenseDto
                {
                    Id = e.Id,
                    Description = e.Description,
                    Amount = e.Amount.Value,
                    Category = e.Category,
                    Date = e.Date,
                    DaysAgo = (int)(now.Date - e.Date.Date).TotalDays
                })
                .ToList();

            // Promedio diario (mes actual)
            var daysElapsed = (now.Date - currentMonthStart.Date).Days + 1;
            var averageDailySpending = daysElapsed > 0 ? totalCurrentMonth / daysElapsed : 0;

            return new DashboardSummaryDto
            {
                TotalCurrentMonth = totalCurrentMonth,
                TotalLastMonth = totalLastMonth,
                PercentageChange = percentageChange,
                ExpenseCountCurrentMonth = expensesCurrentMonth.Count,
                AverageDailySpending = averageDailySpending,
                TopCategory = topCategory?.Category ?? "N/A",
                TopCategoryAmount = topCategory?.Amount ?? 0,
                SpendingByCategory = spendingByCategory,
                MonthlyTrend = monthlyTrend,
                RecentExpenses = recentExpenses
            };
        }
    }
}