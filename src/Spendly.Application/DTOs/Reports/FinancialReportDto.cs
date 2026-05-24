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

        // ── Comparativa con el período anterior ──────────────────────────────────────

        /// <summary>Total de gastos del período anterior (para comparativa).</summary>
        public decimal PrevMonthExpenses { get; set; }

        /// <summary>Total de ingresos del período anterior (para comparativa).</summary>
        public decimal PrevMonthIncomes { get; set; }

        /// <summary>Variación absoluta de gastos vs. período anterior.</summary>
        public decimal ExpenseDelta { get; set; }

        /// <summary>Variación porcentual de gastos vs. período anterior (null si el período anterior es 0).</summary>
        public decimal? ExpenseChangePercent { get; set; }

        /// <summary>Variación absoluta de ingresos vs. período anterior.</summary>
        public decimal IncomeDelta { get; set; }

        /// <summary>Variación porcentual de ingresos vs. período anterior.</summary>
        public decimal? IncomeChangePercent { get; set; }

        // ── Mapa de Calor de Gastos ──────────────────────────────────────────

        /// <summary>Totales diarios de gasto en el período (usado para el heatmap tipo GitHub).</summary>
        public List<DailySpendingDto> DailySpending { get; set; } = new();

        /// <summary>Máximo gasto diario del período. Usado para escalar los colores del heatmap.</summary>
        public decimal MaxDailyAmount { get; set; }

        // ── Insights automáticos ───────────────────────────────────────────

        /// <summary>Insights financieros generados automáticamente por reglas de negocio.</summary>
        public List<InsightDto> Insights { get; set; } = new();
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

    /// <summary>
    /// Representa el total de gastos de un día específico.
    /// Usado para construir el mapa de calor tipo GitHub en la vista de reportes.
    /// </summary>
    public class DailySpendingDto
    {
        /// <summary>Fecha en formato "yyyy-MM-dd".</summary>
        public string Date { get; set; } = string.Empty;

        /// <summary>Suma total de los gastos del día.</summary>
        public decimal Amount { get; set; }

        /// <summary>Número de transacciones en ese día.</summary>
        public int TransactionCount { get; set; }
    }

    /// <summary>
    /// Un insight financiero generado automáticamente por reglas de negocio.
    /// Contiene la clave i18n del mensaje y los parámetros dinámicos para interpolación.
    /// </summary>
    public class InsightDto
    {
        /// <summary>Nivel visual: "positive", "warning", "danger", "info".</summary>
        public string Type { get; set; } = "info";

        /// <summary>Clase de Bootstrap Icons (ej. "bi-piggy-bank-fill").</summary>
        public string Icon { get; set; } = "bi-lightbulb";

        /// <summary>Clave de recurso i18n. Soporta {0}, {1}, {2} como placeholders.</summary>
        public string MessageKey { get; set; } = string.Empty;

        public string Param1 { get; set; } = string.Empty;
        public string Param2 { get; set; } = string.Empty;
        public string Param3 { get; set; } = string.Empty;
    }
}
