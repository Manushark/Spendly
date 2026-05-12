namespace Spendly.Domain.Enums
{
    /// <summary>
    /// Tipos de notificación para alertas del sistema.
    /// </summary>
    public enum NotificationType
    {
        BudgetWarning = 1,              // 80% del presupuesto
        BudgetExceeded = 2,             // 100% del presupuesto
        RecurringExpenseGenerated = 3,  // Gasto recurrente procesado automáticamente
        SavingsGoalMilestone = 4,       // Meta de ahorro alcanzó 50%
        SavingsGoalCompleted = 5        // Meta de ahorro completada
    }
}
