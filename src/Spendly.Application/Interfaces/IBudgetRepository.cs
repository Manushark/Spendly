using Spendly.Domain.Entities;

namespace Spendly.Application.Interfaces
{
    public interface IBudgetRepository
    {
        Task AddAsync(Budget budget);
        Task UpdateAsync(Budget budget);
        Task<bool> DeleteAsync(int id);
        Task<Budget?> GetByIdAsync(int id);
        Task<List<Budget>> GetByUserAndMonthAsync(int userId, int year, int month);
        Task<List<Budget>> GetAllByUserAsync(int userId);
        Task<Budget?> GetByUserCategoryAndMonthAsync(int userId, string category, int year, int month);
    }
}
