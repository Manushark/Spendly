using Spendly.Domain.Entities;

namespace Spendly.Application.Interfaces
{
    public interface IRecurringExpenseRepository
    {
        void Add(RecurringExpense recurringExpense);
        void Update(RecurringExpense recurringExpense);
        bool Delete(int id);
        RecurringExpense? GetById(int id);
        List<RecurringExpense> GetAllByUser(int userId);
        List<RecurringExpense> GetActiveByUser(int userId);
        List<RecurringExpense> GetAllDueForGeneration();
    }
}
