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

            expense.Delete();
            _context.Update(expense);
            await _context.SaveChangesAsync();

            return true;
        }

        // Retrieves an expense by its ID.
        public async Task<Expense?> GetByIdAsync(int id)
        {
            return await _context.Set<Expense>().FirstOrDefaultAsync(e => e.Id == id);
        }

        // Builds the SQL-translatable portion of the filter query.
        // Amount filtering is excluded here because the Money value object
        // uses a value converter that EF Core can't translate in comparisons.
        private IQueryable<Expense> BuildFilteredQuery(
            int userId,
            string? category,
            string? search,
            DateTime? dateFrom,
            DateTime? dateTo)
        {
            var query = _context.Expenses
                .Where(e => e.UserId == userId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(e => e.Category.ToLower() == category.ToLower());

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(e => e.Description.ToLower().Contains(search.ToLower()));

            if (dateFrom.HasValue)
                query = query.Where(e => e.Date >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(e => e.Date <= dateTo.Value);

            return query;
        }

        // Applies amount filters in-memory (can't be done in SQL due to Money value converter).
        private static IEnumerable<Expense> ApplyAmountFilter(
            IEnumerable<Expense> expenses,
            decimal? minAmount,
            decimal? maxAmount)
        {
            if (minAmount.HasValue)
                expenses = expenses.Where(e => e.Amount.Value >= minAmount.Value);

            if (maxAmount.HasValue)
                expenses = expenses.Where(e => e.Amount.Value <= maxAmount.Value);

            return expenses;
        }

        // Retrieves a paginated list of expenses with advanced filtering.
        public async Task<IEnumerable<Expense>> GetAllAsync(
            int userId,
            string? category,
            string? search,
            DateTime? dateFrom,
            DateTime? dateTo,
            decimal? minAmount,
            decimal? maxAmount,
            int page,
            int pageSize)
        {
            var query = BuildFilteredQuery(userId, category, search, dateFrom, dateTo);

            // If no amount filter, let SQL handle ordering + pagination entirely
            if (!minAmount.HasValue && !maxAmount.HasValue)
            {
                return await query
                    .OrderByDescending(e => e.Date)
                    .ThenByDescending(e => e.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }

            // With amount filters: fetch from DB, filter in-memory, then paginate
            var allResults = await query
                .OrderByDescending(e => e.Date)
                .ThenByDescending(e => e.Id)
                .ToListAsync();
            return ApplyAmountFilter(allResults, minAmount, maxAmount)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        // Counts the total number of expenses matching the filters.
        public async Task<int> CountAsync(
            int userId,
            string? category,
            string? search,
            DateTime? dateFrom,
            DateTime? dateTo,
            decimal? minAmount,
            decimal? maxAmount)
        {
            var query = BuildFilteredQuery(userId, category, search, dateFrom, dateTo);

            // If no amount filter, count directly in SQL
            if (!minAmount.HasValue && !maxAmount.HasValue)
                return await query.CountAsync();

            // With amount filters: fetch then count in-memory
            var allResults = await query.ToListAsync();
            return ApplyAmountFilter(allResults, minAmount, maxAmount).Count();
        }

        // ─── Métodos para Dashboard ───

        public async Task<IEnumerable<Expense>> GetByDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
        {
            return await _context.Expenses
                .Where(e => !e.IsDeleted && e.UserId == userId && e.Date >= startDate && e.Date <= endDate)
                .OrderByDescending(e => e.Date)
                .ThenByDescending(e => e.Id)
                .ToListAsync();
        }

        public async Task<Dictionary<string, decimal>> GetTotalByCategoryAsync(int userId, DateTime startDate, DateTime endDate)
        {
            var expenses = await _context.Expenses
                .Where(e => !e.IsDeleted && e.UserId == userId && e.Date >= startDate && e.Date <= endDate)
                .ToListAsync();

            return expenses
                .GroupBy(e => e.Category)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(e => e.Amount.Value)
                );
        }


        public async Task<IEnumerable<Expense>> GetRecentAsync(int userId, int count)
        {
            return await _context.Expenses
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .ThenByDescending(e => e.Id)
                .Take(count)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalAmountAsync(int userId, DateTime startDate, DateTime endDate)
        {
            var expenses = await _context.Expenses
                .Where(e => !e.IsDeleted && e.UserId == userId && e.Date >= startDate && e.Date <= endDate)
                .ToListAsync();

            return expenses.Sum(e => e.Amount.Value);
        }

        public async Task<Dictionary<DateTime, decimal>> GetMonthlyTotalsAsync(int userId, int monthsBack, DateTime? referenceDate = null)
        {
            var reference = referenceDate ?? DateTime.UtcNow;
            var startDate = reference.AddMonths(-monthsBack).Date;

            var expenses = await _context.Expenses
                .Where(e => !e.IsDeleted && e.UserId == userId && e.Date >= startDate)
                .ToListAsync();

            return expenses
                .GroupBy(e => new DateTime(e.Date.Year, e.Date.Month, 1))
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(e => e.Amount.Value)
                );
        }


        public async Task<int> CountByCategoryAsync(int userId, string categoryName)
        {
            return await _context.Expenses
                .CountAsync(e => e.UserId == userId && e.Category.ToLower() == categoryName.ToLower());
        }

        public async Task UpdateCategoryNameAsync(int userId, string oldName, string newName)
        {
            var expenses = await _context.Expenses
                .Where(e => e.UserId == userId && e.Category.ToLower() == oldName.ToLower())
                .ToListAsync();

            foreach (var expense in expenses)
            {
                expense.Update(expense.Amount, expense.Description, expense.Date, newName);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByRecurrenceOnDateAsync(int userId, string description, string category, DateTime date)
        {
            return await _context.Expenses
                .AnyAsync(e =>
                    e.UserId == userId &&
                    e.Description == description &&
                    e.Category.ToLower() == category.ToLower() &&
                    e.Date == date.Date);
        }

        public async Task<string> GetPredominantCurrencyAsync(int userId, string category, DateTime startDate, DateTime endDate)
        {
            var currency = await _context.Expenses
                .Where(e => e.UserId == userId
                    && e.Category.ToLower() == category.ToLower()
                    && e.Date >= startDate && e.Date <= endDate)
                .GroupBy(e => e.Currency)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefaultAsync();

            return currency ?? "USD";
        }
    }
}
