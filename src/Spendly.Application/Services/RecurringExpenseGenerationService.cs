using Spendly.Application.Interfaces;
using Spendly.Domain.Entities;
using Spendly.Domain.Enums;

namespace Spendly.Application.Services
{
    /// <summary>
    /// Servicio que genera gastos a partir de plantillas recurrentes.
    /// </summary>
    public class RecurringExpenseGenerationService
    {
        private readonly IRecurringExpenseRepository _recurringRepo;
        private readonly IExpenseRepository _expenseRepo;
        private readonly INotificationRepository _notificationRepo;

        public RecurringExpenseGenerationService(
            IRecurringExpenseRepository recurringRepo,
            IExpenseRepository expenseRepo,
            INotificationRepository notificationRepo)
        {
            _recurringRepo = recurringRepo;
            _expenseRepo = expenseRepo;
            _notificationRepo = notificationRepo;
        }

        /// <summary>
        /// Genera todos los gastos pendientes de las recurrencias activas.
        /// Retorna la cantidad de gastos generados.
        /// </summary>
        public async Task<int> GeneratePendingExpensesAsync()
        {
            var dueRecurrences = await _recurringRepo.GetAllDueForGenerationAsync();
            var generatedCount = 0;

            foreach (var recurrence in dueRecurrences)
            {
                if (!recurrence.ShouldGenerateToday())
                    continue;

                try
                {
                    await GenerateExpenseFromRecurrenceAsync(recurrence);
                    recurrence.MarkAsGenerated(DateTime.UtcNow.Date);
                    await _recurringRepo.UpdateAsync(recurrence);
                    generatedCount++;

                    // Create notification for the user
                    var notification = Notification.Create(
                        recurrence.UserId,
                        $"🔄 {recurrence.Description}: {recurrence.Amount.Value:N2} → {recurrence.Category}",
                        NotificationType.RecurringExpenseGenerated,
                        recurrence.Id
                    );
                    await _notificationRepo.AddAsync(notification);
                }
                catch (Exception ex)
                {
                    // Log error pero continúa con las demás recurrencias
                    Console.WriteLine($"Error generating expense from recurrence {recurrence.Id}: {ex.Message}");
                }
            }

            return generatedCount;
        }

        /// <summary>
        /// Genera un gasto individual desde una recurrencia.
        /// Verifica que no exista un duplicado antes de insertar.
        /// </summary>
        public async Task GenerateExpenseFromRecurrenceAsync(RecurringExpense recurrence)
        {
            var today = DateTime.UtcNow.Date;

            // Verificar si ya se generó un gasto idéntico hoy (previene duplicados por reinicio del servidor)
            var alreadyExists = await _expenseRepo.ExistsByRecurrenceOnDateAsync(
                recurrence.UserId, recurrence.Description, recurrence.Category, today);

            if (alreadyExists) return;

            var expense = Expense.Create(
                userId: recurrence.UserId,
                description: recurrence.Description,
                amount: recurrence.Amount,
                category: recurrence.Category,
                date: today
            );

            await _expenseRepo.AddAsync(expense);
        }

        /// <summary>
        /// Calcula el total proyectado mensual de todas las recurrencias activas de un usuario.
        /// </summary>
        public async Task<decimal> CalculateMonthlyProjectedTotalAsync(int userId)
        {
            var activeRecurrences = await _recurringRepo.GetActiveByUserAsync(userId);
            decimal total = 0;

            foreach (var recurrence in activeRecurrences)
            {
                var monthlyEquivalent = recurrence.Frequency switch
                {
                    Domain.Enums.RecurrenceFrequency.Daily => recurrence.Amount.Value * 30,
                    Domain.Enums.RecurrenceFrequency.Weekly => recurrence.Amount.Value * 4.33m,
                    Domain.Enums.RecurrenceFrequency.Monthly => recurrence.Amount.Value,
                    Domain.Enums.RecurrenceFrequency.Yearly => recurrence.Amount.Value / 12,
                    _ => 0
                };

                total += monthlyEquivalent;
            }

            return total;
        }
    }
}
