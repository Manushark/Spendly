using Spendly.Application.DTOs.Dashboard;
using Spendly.Application.Interfaces;

namespace Spendly.Application.UseCase.Dashboard
{
    public class GetDashboardStatsUseCase
    {
        private readonly IExpenseRepository _repo;
        private readonly IIncomeRepository _incomeRepo;

        public GetDashboardStatsUseCase(IExpenseRepository repo, IIncomeRepository incomeRepo)
        {
            _repo = repo;
            _incomeRepo = incomeRepo;
        }

        public async Task<DashboardStatsDto> ExecuteAsync(int userId)
        {
            var now = DateTime.UtcNow;
            var currentMonthStart = new DateTime(now.Year, now.Month, 1);
            var currentMonthEnd = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month), 23, 59, 59);

            var previousMonthStart = currentMonthStart.AddMonths(-1);
            var previousMonthEnd = currentMonthStart.AddSeconds(-1);

            // Usar métodos de repositorio que filtran en SQL
            var currentMonthExpenses = (await _repo.GetByDateRangeAsync(userId, currentMonthStart, currentMonthEnd)).ToList();
            var previousMonthExpenses = (await _repo.GetByDateRangeAsync(userId, previousMonthStart, previousMonthEnd)).ToList();

            // Income del mes actual
            var currentMonthIncome = await _incomeRepo.GetTotalAmountAsync(userId, currentMonthStart, currentMonthEnd);

            // Métricas principales
            var currentMonthTotal = currentMonthExpenses.Sum(e => e.Amount.Value);
            var previousMonthTotal = previousMonthExpenses.Sum(e => e.Amount.Value);

            // Balance y tasa de ahorro
            var monthlyBalance = currentMonthIncome - currentMonthTotal;
            var savingsRate = currentMonthIncome > 0
                ? (monthlyBalance / currentMonthIncome) * 100
                : 0m;

            var dailyAverage = currentMonthExpenses.Any()
                ? currentMonthTotal / now.Day
                : 0m;

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

            // Tendencia diaria (últimos 30 días desde HOY) — usar GetByDateRangeAsync en SQL
            var thirtyDaysAgo = now.AddDays(-29).Date;
            var last30DaysExpenses = (await _repo.GetByDateRangeAsync(userId, thirtyDaysAgo, now)).ToList();

            var last30Days = Enumerable.Range(0, 30)
                .Select(i => now.AddDays(-29 + i).Date)
                .ToList();

            var dailyTrend = last30Days
                .Select(date => new DailyTrendDto
                {
                    Date = date.ToString("MMM dd"),
                    Amount = last30DaysExpenses
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
                CurrentMonthIncome = currentMonthIncome,
                MonthlyBalance = monthlyBalance,
                SavingsRate = savingsRate,
                CategoryBreakdown = categoryBreakdown,
                DailyTrend = dailyTrend,
                TopExpenses = topExpenses
            };
        }
    }
}