using Spendly.Application.Interfaces;
using Spendly.Domain.Entities;
using Spendly.Infrastructure.Persistence;

namespace Spendly.Infrastructure.Repositories
{
    public class BudgetRepository : IBudgetRepository
    {
        private readonly SpendlyDbContext _context;

        public BudgetRepository(SpendlyDbContext context) => _context = context;

        public void Add(Budget budget)
        {
            _context.Budgets.Add(budget);
            _context.SaveChanges();
        }

        public void Update(Budget budget)
        {
            _context.Budgets.Update(budget);
            _context.SaveChanges();
        }

        public bool Delete(int id)
        {
            var budget = _context.Budgets.Find(id);
            if (budget == null) return false;

            _context.Budgets.Remove(budget);
            _context.SaveChanges();
            return true;
        }

        public Budget? GetById(int id)
            => _context.Budgets.FirstOrDefault(b => b.Id == id);

        public List<Budget> GetByUserAndMonth(int userId, int year, int month)
            => _context.Budgets
                .Where(b => b.UserId == userId && b.Year == year && b.Month == month)
                .OrderBy(b => b.Category)
                .ToList();

        public List<Budget> GetAllByUser(int userId)
            => _context.Budgets
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.Year)
                .ThenByDescending(b => b.Month)
                .ThenBy(b => b.Category)
                .ToList();

        public Budget? GetByUserCategoryAndMonth(int userId, string category, int year, int month)
            => _context.Budgets
                .FirstOrDefault(b =>
                    b.UserId == userId &&
                    b.Category.ToLower() == category.ToLower() &&
                    b.Year == year &&
                    b.Month == month);
    }
}
