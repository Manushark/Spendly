using Spendly.Application.DTOs.Reports;
using Spendly.Application.Interfaces;

namespace Spendly.Application.UseCase.Reports
{
    /// <summary>
    /// Caso de uso que genera el reporte financiero completo de un usuario para un rango de fechas.
    /// Combina datos de gastos, ingresos y presupuestos para producir tendencias, desglose por categoría
    /// y métricas clave de salud financiera, incluyendo comparativa Budget vs. Actual y período anterior.
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
        /// Ejecuta la generación del reporte financiero para el rango de fechas indicado.
        /// </summary>
        /// <param name="userId">ID del usuario autenticado.</param>
        /// <param name="startDate">Inicio del período (inclusive).</param>
        /// <param name="endDate">Fin del período (inclusive). Se clampea al día actual si es futuro.</param>
        /// <param name="periodLabel">Etiqueta legible del período (ej: "Año a la fecha 2026"). Si vacío, se calcula automáticamente.</param>
        public async Task<FinancialReportDto> ExecuteAsync(
            int      userId,
            DateTime startDate,
            DateTime endDate,
            string   periodLabel = "")
        {
            // Normalizar: nunca ir al futuro
            var today = DateTime.UtcNow.Date;
            if (endDate.Date > today) endDate = today;
            startDate = startDate.Date;

            // ── Período anterior (misma duración, justo antes de startDate) ───────
            var durationDays = Math.Max((endDate - startDate).Days + 1, 1);
            var prevStart    = startDate.AddDays(-durationDays);
            var prevEnd      = startDate.AddDays(-1);

            // ── Totales del período ───────────────────────────────────────────────
            var totalExpenses = await _expenseRepo.GetTotalAmountAsync(userId, startDate, endDate);
            var totalIncomes  = await _incomeRepo.GetTotalAmountAsync(userId, startDate, endDate);
            var netBalance    = totalIncomes - totalExpenses;
            var ratio         = totalIncomes > 0 ? (totalExpenses / totalIncomes) * 100 : 0m;

            // ── Totales del período anterior ──────────────────────────────────────
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

            // ── Desglose por categoría + Budget vs. Actual ────────────────────────
            var categoryTotals   = await _expenseRepo.GetTotalByCategoryAsync(userId, startDate, endDate);
            var expensesInPeriod = (await _expenseRepo.GetByDateRangeAsync(userId, startDate, endDate)).ToList();

            // Presupuesto: se usa el mes de startDate como referencia
            // (para rangos multi-mes se muestra el budget del primer mes del período)
            var budgets = await _budgetRepo.GetByUserAndMonthAsync(userId, startDate.Year, startDate.Month);
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
            var avgDaily = totalExpenses / durationDays;

            // ── Tendencia mensual (dinámica según el rango) ───────────────────────
            // Muestra al menos 6 meses de contexto, y crece con el rango (máx 12 meses).
            var rangeMonths = ((endDate.Year - startDate.Year) * 12) + endDate.Month - startDate.Month + 1;
            var contextBack = Math.Max(6 - rangeMonths, 0);
            var trendFrom   = new DateTime(startDate.Year, startDate.Month, 1).AddMonths(-contextBack);
            var trendTo     = new DateTime(endDate.Year, endDate.Month, 1);

            var trend = new List<MonthlyFinancialTrendDto>();
            for (var m = trendFrom; m <= trendTo; m = m.AddMonths(1))
            {
                if (trend.Count >= 12) break; // Cota máxima
                var ms = m;
                var me = m.AddMonths(1).AddDays(-1);

                var expAmount = await _expenseRepo.GetTotalAmountAsync(userId, ms, me);
                var incAmount = await _incomeRepo.GetTotalAmountAsync(userId, ms, me);

                trend.Add(new MonthlyFinancialTrendDto
                {
                    Month    = m.ToString("MMM yyyy"),
                    Expenses = expAmount,
                    Incomes  = incAmount
                });
            }

            // ── Etiqueta del período ──────────────────────────────────────────────
            var label = !string.IsNullOrEmpty(periodLabel)
                ? periodLabel
                : (startDate.Year == endDate.Year && startDate.Month == endDate.Month)
                    ? startDate.ToString("MMMM yyyy")
                    : $"{startDate:d MMM yyyy} – {endDate:d MMM yyyy}";

            // ── Mapa de Calor: agrupación diaria (reutiliza expensesInPeriod, 0 queries adicionales) ──
            var dailySpending = expensesInPeriod
                .GroupBy(e => e.Date.Date)
                .Select(g => new DailySpendingDto
                {
                    Date             = g.Key.ToString("yyyy-MM-dd"),
                    Amount           = g.Sum(e => e.Amount.Value),
                    TransactionCount = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList();

            var maxDailyAmount = dailySpending.Any() ? dailySpending.Max(d => d.Amount) : 0m;

            return new FinancialReportDto
            {
                PeriodLabel          = label,
                TotalExpenses        = totalExpenses,
                TotalIncomes         = totalIncomes,
                NetBalance           = netBalance,
                ExpenseToIncomeRatio = Math.Round(ratio, 1),
                AverageDailyExpense  = Math.Round(avgDaily, 2),
                TopCategory          = topCategory?.Category ?? "N/A",
                TopCategoryAmount    = topCategory?.Amount ?? 0,
                MonthlyTrend         = trend,
                CategoryBreakdown    = categoryBreakdown,
                // Comparativa período anterior
                PrevMonthExpenses    = prevExpenses,
                PrevMonthIncomes     = prevIncomes,
                ExpenseDelta         = expenseDelta,
                ExpenseChangePercent = expenseChangePercent,
                IncomeDelta          = incomeDelta,
                IncomeChangePercent  = incomeChangePercent,
                // Heatmap
                DailySpending        = dailySpending,
                MaxDailyAmount       = maxDailyAmount
            };
        }
    }
}
