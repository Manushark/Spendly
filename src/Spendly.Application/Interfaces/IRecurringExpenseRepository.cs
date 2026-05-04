using Spendly.Domain.Entities;

namespace Spendly.Application.Interfaces
{
    public interface IRecurringExpenseRepository
    {
        Task AddAsync(RecurringExpense recurringExpense);
        Task UpdateAsync(RecurringExpense recurringExpense);
        Task<bool> DeleteAsync(int id);
        Task<RecurringExpense?> GetByIdAsync(int id);
        Task<List<RecurringExpense>> GetAllByUserAsync(int userId);
        Task<List<RecurringExpense>> GetActiveByUserAsync(int userId);
        Task<List<RecurringExpense>> GetAllDueForGenerationAsync();
        Task<int> CountByCategoryAsync(int userId, string categoryName);
        Task UpdateCategoryNameAsync(int userId, string oldName, string newName);
    }
}
