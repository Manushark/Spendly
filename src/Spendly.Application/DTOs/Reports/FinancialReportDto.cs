namespace Spendly.Application.DTOs.Reports
{
    /// <summary>
    /// DTO raíz que agrupa toda la información de un reporte financiero mensual.
    /// Contiene tendencias históricas, desglose por categoría y métricas de ingresos.
    /// </summary>
    public class FinancialReportDto
    {
        /// <summary>Nombre del período cubierto, ej: "Mayo 2026".</summary>
        public string PeriodLabel { get; set; } = string.Empty;

        /// <summary>Total gastado en el mes de referencia.</summary>
        public decimal TotalExpenses { get; set; }

        /// <summary>Total de ingresos en el mes de referencia.</summary>
        public decimal TotalIncomes { get; set; }

        /// <summary>Balance neto del mes (Ingresos - Gastos).</summary>
        public decimal NetBalance { get; set; }

        /// <summary>Porcentaje del ingreso que fue consumido por gastos.</summary>
        public decimal ExpenseToIncomeRatio { get; set; }

        /// <summary>Tendencia mensual de gastos e ingresos (últimos 6 meses).</summary>
        public List<MonthlyFinancialTrendDto> MonthlyTrend { get; set; } = new();

        /// <summary>Desglose de gastos por categoría del mes.</summary>
        public List<CategoryReportItemDto> CategoryBreakdown { get; set; } = new();

        /// <summary>Categoría con mayor gasto del mes.</summary>
        public string TopCategory { get; set; } = string.Empty;

        /// <summary>Monto total de la categoría con mayor gasto.</summary>
        public decimal TopCategoryAmount { get; set; }

        /// <summary>Promedio diario de gasto durante el mes.</summary>
        public decimal AverageDailyExpense { get; set; }

        // ── Comparativa con el mes anterior ─────────────────────────────────────

        /// <summary>Total de gastos del mes anterior (para comparativa).</summary>
        public decimal PrevMonthExpenses { get; set; }

        /// <summary>Total de ingresos del mes anterior (para comparativa).</summary>
        public decimal PrevMonthIncomes { get; set; }

        /// <summary>Variación absoluta de gastos vs. mes anterior.</summary>
        public decimal ExpenseDelta { get; set; }

        /// <summary>Variación porcentual de gastos vs. mes anterior (puede ser null si el mes anterior es 0).</summary>
        public decimal? ExpenseChangePercent { get; set; }

        /// <summary>Variación absoluta de ingresos vs. mes anterior.</summary>
        public decimal IncomeDelta { get; set; }

        /// <summary>Variación porcentual de ingresos vs. mes anterior.</summary>
        public decimal? IncomeChangePercent { get; set; }
    }

    /// <summary>
    /// Representa los totales de gastos e ingresos para un mes específico.
    /// Usado en el gráfico de líneas de tendencia.
    /// </summary>
    public class MonthlyFinancialTrendDto
    {
        /// <summary>Etiqueta legible del mes, ej: "Ene 2026".</summary>
        public string Month { get; set; } = string.Empty;

        /// <summary>Total de gastos del mes.</summary>
        public decimal Expenses { get; set; }

        /// <summary>Total de ingresos del mes.</summary>
        public decimal Incomes { get; set; }
    }

    /// <summary>
    /// Detalle de gasto para una categoría específica dentro del período del reporte.
    /// Usado en el gráfico de dona y la tabla de desglose.
    /// </summary>
    public class CategoryReportItemDto
    {
        /// <summary>Nombre de la categoría.</summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>Monto total gastado en esta categoría.</summary>
        public decimal Amount { get; set; }

        /// <summary>Porcentaje que representa del total de gastos del mes.</summary>
        public decimal Percentage { get; set; }

        /// <summary>Cantidad de transacciones en esta categoría.</summary>
        public int TransactionCount { get; set; }

        // ── Budget vs. Actual ────────────────────────────────────────────

        /// <summary>Límite mensual del presupuesto para esta categoría. Null si no tiene presupuesto definido.</summary>
        public decimal? BudgetLimit { get; set; }

        /// <summary>Porcentaje del presupuesto consumido (Amount / BudgetLimit * 100). Null si no hay presupuesto.</summary>
        public decimal? BudgetUsagePercent { get; set; }

        /// <summary>True si el gasto superó el límite del presupuesto.</summary>
        public bool IsBudgetExceeded => BudgetLimit.HasValue && Amount > BudgetLimit.Value;
    }
}
