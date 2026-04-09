using Microsoft.EntityFrameworkCore;
using Spendly.Application.Interfaces;
using Spendly.Domain.Entities;
using Spendly.Infrastructure.Persistence;

namespace Spendly.Infrastructure.Repositories
{
    public class BudgetRepository : IBudgetRepository
    {
        private readonly SpendlyDbContext _context;

        public BudgetRepository(SpendlyDbContext context) => _context = context;

        public async Task AddAsync(Budget budget)
        {
            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Budget budget)
        {
            _context.Budgets.Update(budget);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null) return false;

            _context.Budgets.Remove(budget);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Budget?> GetByIdAsync(int id)
            => await _context.Budgets.FirstOrDefaultAsync(b => b.Id == id);

        public async Task<List<Budget>> GetByUserAndMonthAsync(int userId, int year, int month)
            => await _context.Budgets
                .Where(b => b.UserId == userId && b.Year == year && b.Month == month)
                .OrderBy(b => b.Category)
                .ToListAsync();

        public async Task<List<Budget>> GetAllByUserAsync(int userId)
            => await _context.Budgets
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.Year)
                .ThenByDescending(b => b.Month)
                .ThenBy(b => b.Category)
                .ToListAsync();

        public async Task<Budget?> GetByUserCategoryAndMonthAsync(int userId, string category, int year, int month)
            => await _context.Budgets
                .FirstOrDefaultAsync(b =>
                    b.UserId == userId &&
                    b.Category.ToLower() == category.ToLower() &&
                    b.Year == year &&
                    b.Month == month);
    }
}
