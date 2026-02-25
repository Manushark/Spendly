using Spendly.Application.DTOs.Dashboard;
using Spendly.Application.Interfaces;

namespace Spendly.Application.UseCase.Dashboard
{
    public class GetDashboardStatsUseCase
    {
        private readonly IExpenseRepository _repo;

        public GetDashboardStatsUseCase(IExpenseRepository repo) => _repo = repo;

        public DashboardStatsDto Execute(int userId)
        {
            var now = DateTime.UtcNow;
            var currentMonthStart = new DateTime(now.Year, now.Month, 1);
            var previousMonthStart = currentMonthStart.AddMonths(-1);
            var previousMonthEnd = currentMonthStart.AddDays(-1);

            // Obtener todos los gastos del usuario (sin límite de paginación)
            var allExpenses = _repo.GetAll(userId, category: null, page: 1, pageSize: 10000);

            var currentMonthExpenses = allExpenses
                .Where(e => e.Date >= currentMonthStart && e.Date <= now)
                .ToList();

            var previousMonthExpenses = allExpenses
                .Where(e => e.Date >= previousMonthStart && e.Date <= previousMonthEnd)
                .ToList();

            // Métricas principales
            var currentMonthTotal = currentMonthExpenses.Sum(e => e.Amount.Value);
            var previousMonthTotal = previousMonthExpenses.Sum(e => e.Amount.Value);
            var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
            var dailyAverage = currentMonthTotal / now.Day;

            var highestExpense = currentMonthExpenses.Any()
                ? currentMonthExpenses.Max(e => e.Amount.Value)
                : 0m;

            // Categoría con más gasto
            var topCategory = currentMonthExpenses
                .GroupBy(e => e.Category)
                .OrderByDescending(g => g.Sum(e => e.Amount.Value))
                .FirstOrDefault()?.Key ?? "N/A";

            // Desglose por categoría (mes actual)
            var categoryBreakdown = currentMonthExpenses
                .GroupBy(e => e.Category)
                .Select(g => new CategoryStatsDto
                {
                    Category = g.Key,
                    Total = g.Sum(e => e.Amount.Value),
                    Count = g.Count(),
                    Percentage = currentMonthTotal > 0
                        ? (g.Sum(e => e.Amount.Value) / currentMonthTotal) * 100
                        : 0
                })
                .OrderByDescending(c => c.Total)
                .ToList();

            // Tendencia diaria (últimos 30 días)
            var last30Days = Enumerable.Range(0, 30)
                .Select(i => now.AddDays(-29 + i).Date)
                .ToList();

            var dailyTrend = last30Days
                .Select(date => new DailyTrendDto
                {
                    Date = date.ToString("MMM dd"),
                    Amount = allExpenses
                        .Where(e => e.Date.Date == date)
                        .Sum(e => e.Amount.Value)
                })
                .ToList();

            // Top 5 gastos del mes
            var topExpenses = currentMonthExpenses
                .OrderByDescending(e => e.Amount.Value)
                .Take(5)
                .Select(e => new TopExpenseDto
                {
                    Id = e.Id,
                    Description = e.Description,
                    Amount = e.Amount.Value,
                    Category = e.Category,
                    Date = e.Date
                })
                .ToList();

            return new DashboardStatsDto
            {
                CurrentMonthTotal = currentMonthTotal,
                PreviousMonthTotal = previousMonthTotal,
                DailyAverage = dailyAverage,
                HighestExpense = highestExpense,
                TopCategory = topCategory,
                TotalExpensesCount = currentMonthExpenses.Count,
                CategoryBreakdown = categoryBreakdown,
                DailyTrend = dailyTrend,
                TopExpenses = topExpenses
            };
        }
    }
}