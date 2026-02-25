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
        public void Add(Expense expense)
        {
            _context.Add(expense);
            _context.SaveChanges();
        }

        // Updates an existing expense in the database.
        public void Update(Expense expense)
        {
            _context.Expenses.Update(expense);
            _context.SaveChanges();
        }

        // Deletes an expense by its ID. Returns true if deletion was successful, false if the expense was not found.
        public bool Delete(int id)
        {
            var expense = _context.Set<Expense>().Find(id);
            if (expense == null) return false;

            _context.Remove(expense);
            _context.SaveChanges();

            return true;
        }

        // Retrieves an expense by its ID.
        public Expense? GetById(int id)
        {
            return _context.Set<Expense>().FirstOrDefault(e => e.Id == id);
        }

        // Retrieves a paginated list of expenses for a specific user, optionally filtered by category.
        public IEnumerable<Expense> GetAll(int userId, string? category, int page, int pageSize)
        {
            var query = _context.Expenses
                .Where(e => e.UserId == userId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(e => e.Category.ToLower() == category.ToLower());

            return query
                .OrderByDescending(e => e.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        // Counts the total number of expenses for a given user and optional category filter.
        public int Count(int userId, string? category)
        {
            var query = _context.Expenses
                .Where(e => e.UserId == userId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(e => e.Category.ToLower() == category.ToLower());

            return query.Count();
        }

        // ─── Métodos para Dashboard ───

        public IEnumerable<Expense> GetByDateRange(int userId, DateTime startDate, DateTime endDate)
        {
            return _context.Expenses
                .Where(e => e.UserId == userId && e.Date >= startDate && e.Date <= endDate)
                .OrderByDescending(e => e.Date)
                .ToList();
        }

        public Dictionary<string, decimal> GetTotalByCategory(int userId, DateTime startDate, DateTime endDate)
        {
            return _context.Expenses
                .Where(e => e.UserId == userId && e.Date >= startDate && e.Date <= endDate)
                .GroupBy(e => e.Category)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(e => e.Amount.Value)
                );
        }

        public IEnumerable<Expense> GetRecent(int userId, int count)
        {
            return _context.Expenses
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .Take(count)
                .ToList();
        }

        public decimal GetTotalAmount(int userId, DateTime startDate, DateTime endDate)
        {
            return _context.Expenses
                .Where(e => e.UserId == userId && e.Date >= startDate && e.Date <= endDate)
                .Sum(e => (decimal?)e.Amount.Value) ?? 0m;
        }

        public Dictionary<DateTime, decimal> GetMonthlyTotals(int userId, int monthsBack)
        {
            var startDate = DateTime.UtcNow.AddMonths(-monthsBack).Date;

            return _context.Expenses
                .Where(e => e.UserId == userId && e.Date >= startDate)
                .GroupBy(e => new DateTime(e.Date.Year, e.Date.Month, 1))
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(e => e.Amount.Value)
                );
        }
    }
}

// Retrieves all expenses from the database.
//public List<Expense> GetAll()
//{
//    return _context.Set<Expense>().ToList();
//}