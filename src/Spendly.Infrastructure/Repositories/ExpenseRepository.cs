using Microsoft.EntityFrameworkCore;
using Spendly.Application.Interfaces;
using Spendly.Domain.Entities;
using Spendly.Infrastructure.Persistence;

namespace Spendly.Infrastructure.Repositories
{
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly SpendlyDbContext _context;

        public ExpenseRepository(SpendlyDbContext dbContext)
        {
            _context = dbContext;
        }

        // Adds a new expense.
        public async Task AddAsync(Expense expense)
        {
            _context.Add(expense);
            await _context.SaveChangesAsync();
        }

        // Updates an existing expense in the database.
        public async Task UpdateAsync(Expense expense)
        {
            _context.Expenses.Update(expense);
            await _context.SaveChangesAsync();
        }

        // Deletes an expense by its ID. Returns true if deletion was successful, false if the expense was not found.
        public async Task<bool> DeleteAsync(int id)
        {
            var expense = await _context.Set<Expense>().FindAsync(id);
            if (expense == null) return false;

            _context.Remove(expense);
            await _context.SaveChangesAsync();

            return true;
        }

        // Retrieves an expense by its ID.
        public async Task<Expense?> GetByIdAsync(int id)
        {
            return await _context.Set<Expense>().FirstOrDefaultAsync(e => e.Id == id);
        }

        // Retrieves a paginated list of expenses for a specific user, optionally filtered by category.
        public async Task<IEnumerable<Expense>> GetAllAsync(int userId, string? category, int page, int pageSize)
        {
            var query = _context.Expenses
                .Where(e => e.UserId == userId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(e => e.Category.ToLower() == category.ToLower());

            return await query
                .OrderByDescending(e => e.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // Counts the total number of expenses for a given user and optional category filter.
        public async Task<int> CountAsync(int userId, string? category)
        {
            var query = _context.Expenses
                .Where(e => e.UserId == userId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(e => e.Category.ToLower() == category.ToLower());

            return await query.CountAsync();
        }

        // ─── Métodos para Dashboard ───

        public async Task<IEnumerable<Expense>> GetByDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
        {
            return await _context.Expenses
                .Where(e => e.UserId == userId && e.Date >= startDate && e.Date <= endDate)
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        public async Task<Dictionary<string, decimal>> GetTotalByCategoryAsync(int userId, DateTime startDate, DateTime endDate)
        {
            return await _context.Expenses
                .Where(e => e.UserId == userId && e.Date >= startDate && e.Date <= endDate)
                .GroupBy(e => e.Category)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Sum(e => e.Amount.Value)
                );
        }

        public async Task<IEnumerable<Expense>> GetRecentAsync(int userId, int count)
        {
            return await _context.Expenses
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .Take(count)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalAmountAsync(int userId, DateTime startDate, DateTime endDate)
        {
            return await _context.Expenses
                .Where(e => e.UserId == userId && e.Date >= startDate && e.Date <= endDate)
                .SumAsync(e => (decimal?)e.Amount.Value) ?? 0m;
        }

        public async Task<Dictionary<DateTime, decimal>> GetMonthlyTotalsAsync(int userId, int monthsBack)
        {
            var startDate = DateTime.UtcNow.AddMonths(-monthsBack).Date;

            return await _context.Expenses
                .Where(e => e.UserId == userId && e.Date >= startDate)
                .GroupBy(e => new DateTime(e.Date.Year, e.Date.Month, 1))
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Sum(e => e.Amount.Value)
                );
        }
    }
}