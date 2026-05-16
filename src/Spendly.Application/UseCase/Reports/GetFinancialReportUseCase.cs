using Spendly.Application.DTOs.Reports;
using Spendly.Application.Interfaces;

namespace Spendly.Application.UseCase.Reports
{
    /// <summary>
    /// Caso de uso que genera el reporte financiero completo de un usuario para un mes dado.
    /// Combina datos de gastos, ingresos y presupuestos para producir tendencias, desglose por categoría
    /// y métricas clave de salud financiera, incluyendo comparativa Budget vs. Actual.
    /// </summary>
    public class GetFinancialReportUseCase
    {
        private readonly IExpenseRepository  _expenseRepo;
        private readonly IIncomeRepository   _incomeRepo;
        private readonly IBudgetRepository   _budgetRepo;

        public GetFinancialReportUseCase(
            IExpenseRepository  expenseRepo,
            IIncomeRepository   incomeRepo,
            IBudgetRepository   budgetRepo)
        {
            _expenseRepo = expenseRepo;
            _incomeRepo  = incomeRepo;
            _budgetRepo  = budgetRepo;
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

            // ── Rango del mes anterior (para comparativa) ─────────────────────────
            var prevStart = periodStart.AddMonths(-1);
            var prevEnd   = prevStart.AddMonths(1).AddDays(-1);

            // ── Totales del período ───────────────────────────────────────────────
            var totalExpenses = await _expenseRepo.GetTotalAmountAsync(userId, periodStart, periodEnd);
            var totalIncomes  = await _incomeRepo.GetTotalAmountAsync(userId, periodStart, periodEnd);
            var netBalance    = totalIncomes - totalExpenses;
            var ratio         = totalIncomes > 0 ? (totalExpenses / totalIncomes) * 100 : 0m;

            // ── Totales del mes anterior ──────────────────────────────────────────
            var prevExpenses = await _expenseRepo.GetTotalAmountAsync(userId, prevStart, prevEnd);
            var prevIncomes  = await _incomeRepo.GetTotalAmountAsync(userId, prevStart, prevEnd);

            var expenseDelta         = totalExpenses - prevExpenses;
            var expenseChangePercent = prevExpenses > 0
                ? Math.Round((expenseDelta / prevExpenses) * 100, 1)
                : (decimal?)null;

            var incomeDelta         = totalIncomes - prevIncomes;
            var incomeChangePercent = prevIncomes > 0
                ? Math.Round((incomeDelta / prevIncomes) * 100, 1)
                : (decimal?)null;

            // ── Desglose por categoría + Budget vs. Actual ─────────────────────
            var categoryTotals   = await _expenseRepo.GetTotalByCategoryAsync(userId, periodStart, periodEnd);
            var expensesInPeriod = (await _expenseRepo.GetByDateRangeAsync(userId, periodStart, periodEnd)).ToList();

            // Cargar todos los presupuestos del mes de una sola vez (1 query)
            var budgets = await _budgetRepo.GetByUserAndMonthAsync(userId, year, month);
            var budgetByCategory = budgets.ToDictionary(
                b => b.Category,
                b => b.MonthlyLimit,
                StringComparer.OrdinalIgnoreCase);

            var categoryBreakdown = categoryTotals
                .Select(kvp =>
                {
                    var budgetLimit = budgetByCategory.TryGetValue(kvp.Key, out var limit)
                        ? (decimal?)limit
                        : null;

                    var usagePct = budgetLimit.HasValue && budgetLimit.Value > 0
                        ? Math.Round((kvp.Value / budgetLimit.Value) * 100, 1)
                        : (decimal?)null;

                    return new CategoryReportItemDto
                    {
                        Category         = kvp.Key,
                        Amount           = kvp.Value,
                        Percentage       = totalExpenses > 0 ? Math.Round((kvp.Value / totalExpenses) * 100, 1) : 0,
                        TransactionCount = expensesInPeriod.Count(e =>
                            e.Category.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase)),
                        BudgetLimit        = budgetLimit,
                        BudgetUsagePercent = usagePct
                    };
                })
                .OrderByDescending(c => c.Amount)
                .ToList();

            var topCategory = categoryBreakdown.FirstOrDefault();

            // ── Promedio diario ───────────────────────────────────────────────────
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
                CategoryBreakdown    = categoryBreakdown,
                // Comparativa mes anterior
                PrevMonthExpenses    = prevExpenses,
                PrevMonthIncomes     = prevIncomes,
                ExpenseDelta         = expenseDelta,
                ExpenseChangePercent = expenseChangePercent,
                IncomeDelta          = incomeDelta,
                IncomeChangePercent  = incomeChangePercent
            };
        }
    }
}
