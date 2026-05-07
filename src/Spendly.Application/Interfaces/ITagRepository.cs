using Spendly.Domain.Entities;

namespace Spendly.Application.Interfaces
{
    public interface ITagRepository
    {
        Task AddAsync(Tag tag);
        Task UpdateAsync(Tag tag);
        Task<bool> DeleteAsync(int id);
        Task<Tag?> GetByIdAsync(int id);
        Task<List<Tag>> GetAllByUserAsync(int userId);
        Task<Tag?> GetByNameAsync(int userId, string name);

        // ExpenseTag operations
        Task SetExpenseTagsAsync(int userId, int expenseId, List<int> tagIds);
        Task<List<Tag>> GetTagsForExpenseAsync(int expenseId);
        Task<Dictionary<int, List<Tag>>> GetTagsForExpensesAsync(List<int> expenseIds);
    }
}
