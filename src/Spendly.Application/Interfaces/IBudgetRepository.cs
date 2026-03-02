using Spendly.Domain.Entities;

namespace Spendly.Application.Interfaces
{
    public interface IBudgetRepository
    {
        void Add(Budget budget);
        void Update(Budget budget);
        bool Delete(int id);
        Budget? GetById(int id);
        List<Budget> GetByUserAndMonth(int userId, int year, int month);
        List<Budget> GetAllByUser(int userId);
        Budget? GetByUserCategoryAndMonth(int userId, string category, int year, int month);
    }
}
