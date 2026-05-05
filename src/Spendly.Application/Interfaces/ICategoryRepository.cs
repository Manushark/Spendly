using Spendly.Domain.Entities;

namespace Spendly.Application.Interfaces
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetAllByUserAsync(int userId);
        Task<Category?> GetByIdAsync(int id);
        Task<Category?> GetByNameAndUserAsync(int userId, string name);
        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task<bool> DeleteAsync(int id);
        Task SeedDefaultsAsync(int userId);
        Task<int> CountByUserAsync(int userId);
    }
}
