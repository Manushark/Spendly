using Microsoft.EntityFrameworkCore;
using Spendly.Application.Interfaces;
using Spendly.Domain.Entities;
using Spendly.Infrastructure.Persistence;

namespace Spendly.Infrastructure.Repositories
{
    public class IncomeRepository : IIncomeRepository
    {
        private readonly SpendlyDbContext _context;

        public IncomeRepository(SpendlyDbContext context) => _context = context;

        public async Task AddAsync(Income income)
        {
            _context.Incomes.Add(income);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Income income)
        {
            _context.Incomes.Update(income);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var income = await _context.Incomes.FindAsync(id);
            if (income == null) return false;

            _context.Incomes.Remove(income);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Income?> GetByIdAsync(int id)
        {
            return await _context.Incomes.FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<IEnumerable<Income>> GetAllByUserAsync(int userId, int page, int pageSize)
        {
            return await _context.Incomes
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountByUserAsync(int userId)
        {
            return await _context.Incomes.CountAsync(i => i.UserId == userId);
        }

        public async Task<IEnumerable<Income>> GetByDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
        {
            return await _context.Incomes
                .Where(i => i.UserId == userId && i.Date >= startDate && i.Date <= endDate)
                .OrderByDescending(i => i.Date)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalAmountAsync(int userId, DateTime startDate, DateTime endDate)
        {
            // Money is a Value Object that EF Core can't translate in aggregate queries.
            // Materialize first, then sum in-memory (same approach as ExpenseRepository).
            var incomes = await _context.Incomes
                .Where(i => i.UserId == userId && i.Date >= startDate && i.Date <= endDate)
                .ToListAsync();

            return incomes.Sum(i => i.Amount.Value);
        }
    }
}
