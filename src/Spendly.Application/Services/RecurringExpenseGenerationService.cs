using Spendly.Application.Interfaces;
using Spendly.Domain.Entities;

namespace Spendly.Application.Services
{
    /// <summary>
    /// Servicio que genera gastos a partir de plantillas recurrentes.
    /// </summary>
    public class RecurringExpenseGenerationService
    {
        private readonly IRecurringExpenseRepository _recurringRepo;
        private readonly IExpenseRepository _expenseRepo;

        public RecurringExpenseGenerationService(
            IRecurringExpenseRepository recurringRepo,
            IExpenseRepository expenseRepo)
        {
            _recurringRepo = recurringRepo;
            _expenseRepo = expenseRepo;
        }

        /// <summary>
        /// Genera todos los gastos pendientes de las recurrencias activas.
        /// Retorna la cantidad de gastos generados.
        /// </summary>
        public int GeneratePendingExpenses()
        {
            var dueRecurrences = _recurringRepo.GetAllDueForGeneration();
            var generatedCount = 0;

            foreach (var recurrence in dueRecurrences)
            {
                if (!recurrence.ShouldGenerateToday())
                    continue;

                try
                {
                    GenerateExpenseFromRecurrence(recurrence);
                    recurrence.MarkAsGenerated(DateTime.Today);
                    _recurringRepo.Update(recurrence);
                    generatedCount++;
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
        /// </summary>
        public void GenerateExpenseFromRecurrence(RecurringExpense recurrence)
        {
            var expense = Expense.Create(
                userId: recurrence.UserId,
                description: recurrence.Description,
                amount: recurrence.Amount.Value,
                category: recurrence.Category,
                date: DateTime.Today
            );

            _expenseRepo.Add(expense);
        }

        /// <summary>
        /// Calcula el total proyectado mensual de todas las recurrencias activas de un usuario.
        /// </summary>
        public decimal CalculateMonthlyProjectedTotal(int userId)
        {
            var activeRecurrences = _recurringRepo.GetActiveByUser(userId);
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
