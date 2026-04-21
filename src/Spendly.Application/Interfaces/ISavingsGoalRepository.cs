using Spendly.Domain.Entities;

namespace Spendly.Application.Interfaces
{
    public interface ISavingsGoalRepository
    {
        Task AddAsync(SavingsGoal goal);
        Task UpdateAsync(SavingsGoal goal);
        Task<bool> DeleteAsync(int id);
        Task<SavingsGoal?> GetByIdAsync(int id);
        Task<List<SavingsGoal>> GetAllByUserAsync(int userId);
    }
}
