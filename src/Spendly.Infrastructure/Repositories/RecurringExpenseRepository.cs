using Microsoft.EntityFrameworkCore;
using Spendly.Application.Interfaces;
using Spendly.Domain.Entities;
using Spendly.Infrastructure.Persistence;

namespace Spendly.Infrastructure.Repositories
{
    public class RecurringExpenseRepository : IRecurringExpenseRepository
    {
        private readonly SpendlyDbContext _context;

        public RecurringExpenseRepository(SpendlyDbContext context) => _context = context;

        public async Task AddAsync(RecurringExpense recurringExpense)
        {
            _context.RecurringExpenses.Add(recurringExpense);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(RecurringExpense recurringExpense)
        {
            _context.RecurringExpenses.Update(recurringExpense);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var recurring = await _context.RecurringExpenses.FindAsync(id);
            if (recurring == null) return false;

            _context.RecurringExpenses.Remove(recurring);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<RecurringExpense?> GetByIdAsync(int id)
            => await _context.RecurringExpenses.FirstOrDefaultAsync(r => r.Id == id);

        public async Task<List<RecurringExpense>> GetAllByUserAsync(int userId)
            => await _context.RecurringExpenses
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

        public async Task<List<RecurringExpense>> GetActiveByUserAsync(int userId)
            => await _context.RecurringExpenses
                .Where(r => r.UserId == userId && r.IsActive)
                .ToListAsync();

        public async Task<List<RecurringExpense>> GetAllDueForGenerationAsync()
        {
            var today = DateTime.Today;

            return await _context.RecurringExpenses
                .Where(r => r.IsActive && r.StartDate <= today)
                .Where(r => !r.EndDate.HasValue || r.EndDate.Value >= today)
                .ToListAsync();
        }
    }
}
