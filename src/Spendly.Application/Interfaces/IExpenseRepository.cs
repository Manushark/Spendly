using Spendly.Domain.Entities;

namespace Spendly.Application.Interfaces
{
    // Repository interface for managing Expense entities
    public interface IExpenseRepository
    {
        void Add(Expense expense);
        bool Delete(int id);
        void Update(Expense expense);

        Expense? GetById(int id);

        IEnumerable<Expense> GetAll(
            int userId,
            string? category,
            int page,
            int pageSize
            );

        int Count(int userId, string? category);
    }
}
