using Spendly.Application.DTOs.Reports;
using Spendly.Application.Interfaces;

namespace Spendly.Application.UseCase.Reports
{
    /// <summary>
    /// Caso de uso que genera el reporte financiero completo de un usuario para un mes dado.
    /// Combina datos de gastos e ingresos para producir tendencias, desglose por categoría
    /// y métricas clave de salud financiera.
    /// </summary>
    public class GetFinancialReportUseCase
    {
        private readonly IExpenseRepository _expenseRepo;
        private readonly IIncomeRepository _incomeRepo;

        public GetFinancialReportUseCase(
            IExpenseRepository expenseRepo,
            IIncomeRepository incomeRepo)
        {
            _expenseRepo = expenseRepo;
            _incomeRepo = incomeRepo;
        }

        /// <summary>
        /// Ejecuta la generación del reporte financiero para el mes y año indicados.
        /// </summary>
        /// <param name="userId">ID del usuario autenticado.</param>
        /// <param name="year">Año del reporte (ej: 2026).</param>
        /// <param name="month">Mes del reporte (1–12).</param>
        public async Task<FinancialReportDto> ExecuteAsync(int userId, int year, int month)
        {
            // ── Rango del mes de referencia ──────────────────────────────────────
            var periodStart = new DateTime(year, month, 1);
            var periodEnd   = periodStart.AddMonths(1).AddDays(-1);

            // ── Totales del período ───────────────────────────────────────────────
            var totalExpenses = await _expenseRepo.GetTotalAmountAsync(userId, periodStart, periodEnd);
            var totalIncomes  = await _incomeRepo.GetTotalAmountAsync(userId, periodStart, periodEnd);
            var netBalance    = totalIncomes - totalExpenses;
            var ratio         = totalIncomes > 0 ? (totalExpenses / totalIncomes) * 100 : 0m;

            // ── Desglose por categoría ────────────────────────────────────────────
            var categoryTotals     = await _expenseRepo.GetTotalByCategoryAsync(userId, periodStart, periodEnd);
            var expensesInPeriod   = (await _expenseRepo.GetByDateRangeAsync(userId, periodStart, periodEnd)).ToList();

            var categoryBreakdown = categoryTotals
                .Select(kvp => new CategoryReportItemDto
                {
                    Category         = kvp.Key,
                    Amount           = kvp.Value,
                    Percentage       = totalExpenses > 0 ? Math.Round((kvp.Value / totalExpenses) * 100, 1) : 0,
                    TransactionCount = expensesInPeriod.Count(e =>
                        e.Category.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase))
                })
                .OrderByDescending(c => c.Amount)
                .ToList();

            var topCategory = categoryBreakdown.FirstOrDefault();

            // ── Promedio diario ───────────────────────────────────────────────────
            // Si el mes solicitado es el mes actual, solo dividir entre días transcurridos.
            var today      = DateTime.UtcNow.Date;
            var daysInCalc = (year == today.Year && month == today.Month)
                ? (today - periodStart.Date).Days + 1
                : DateTime.DaysInMonth(year, month);
            var avgDaily = daysInCalc > 0 ? totalExpenses / daysInCalc : 0m;

            // ── Tendencia mensual (últimos 6 meses) ───────────────────────────────
            var trend = new List<MonthlyFinancialTrendDto>();
            for (int i = 5; i >= 0; i--)
            {
                var trendDate  = new DateTime(year, month, 1).AddMonths(-i);
                var trendStart = trendDate;
                var trendEnd   = trendStart.AddMonths(1).AddDays(-1);

                var expAmount = await _expenseRepo.GetTotalAmountAsync(userId, trendStart, trendEnd);
                var incAmount = await _incomeRepo.GetTotalAmountAsync(userId, trendStart, trendEnd);

                trend.Add(new MonthlyFinancialTrendDto
                {
                    Month    = trendDate.ToString("MMM yyyy"),
                    Expenses = expAmount,
                    Incomes  = incAmount
                });
            }

            return new FinancialReportDto
            {
                PeriodLabel          = periodStart.ToString("MMMM yyyy"),
                TotalExpenses        = totalExpenses,
                TotalIncomes         = totalIncomes,
                NetBalance           = netBalance,
                ExpenseToIncomeRatio = Math.Round(ratio, 1),
                AverageDailyExpense  = Math.Round(avgDaily, 2),
                TopCategory          = topCategory?.Category ?? "N/A",
                TopCategoryAmount    = topCategory?.Amount ?? 0,
                MonthlyTrend         = trend,
                CategoryBreakdown    = categoryBreakdown
            };
        }
    }
}
