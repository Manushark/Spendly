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
            // Usar hora local en vez de UTC para evitar problemas de zona horaria
            var now = DateTime.Now;
            var currentMonthStart = new DateTime(now.Year, now.Month, 1);
            var currentMonthEnd = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month), 23, 59, 59);

            var previousMonthStart = currentMonthStart.AddMonths(-1);
            var previousMonthEnd = currentMonthStart.AddSeconds(-1);

            // Obtener TODOS los gastos del usuario (últimos 12 meses para el gráfico)
            var allExpenses = _repo.GetAll(userId, category: null, page: 1, pageSize: 10000)
                .Where(e => e.Date >= now.AddMonths(-12))  // Solo últimos 12 meses
                .ToList();

            // Filtrar por mes actual (comparando solo año y mes, no la hora)
            var currentMonthExpenses = allExpenses
                .Where(e => e.Date.Year == now.Year && e.Date.Month == now.Month)
                .ToList();

            var previousMonthExpenses = allExpenses
                .Where(e => e.Date.Year == previousMonthStart.Year && e.Date.Month == previousMonthStart.Month)
                .ToList();

            // Métricas principales
            var currentMonthTotal = currentMonthExpenses.Sum(e => e.Amount.Value);
            var previousMonthTotal = previousMonthExpenses.Sum(e => e.Amount.Value);

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

            // Tendencia diaria (últimos 30 días desde HOY)
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