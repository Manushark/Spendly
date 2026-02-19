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
    }
}
        // Retrieves all expenses from the database.
        //public List<Expense> GetAll()
        //{
        //    return _context.Set<Expense>().ToList();
        //}