using Spendly.Domain.Entities;

namespace Spendly.Application.Interfaces
{
    public interface IIncomeRepository
    {
        Task AddAsync(Income income);
        Task UpdateAsync(Income income);
        Task<bool> DeleteAsync(int id);
        Task<Income?> GetByIdAsync(int id);
        Task<IEnumerable<Income>> GetAllByUserAsync(int userId, int page, int pageSize);
        Task<int> CountByUserAsync(int userId);
        Task<IEnumerable<Income>> GetByDateRangeAsync(int userId, DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalAmountAsync(int userId, DateTime startDate, DateTime endDate);
    }
}
