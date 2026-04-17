namespace Spendly.Domain.Enums
{
    /// <summary>
    /// Tipos de notificación para alertas del sistema.
    /// </summary>
    public enum NotificationType
    {
        BudgetWarning = 1,   // 80% del presupuesto
        BudgetExceeded = 2   // 100% del presupuesto
    }
}
