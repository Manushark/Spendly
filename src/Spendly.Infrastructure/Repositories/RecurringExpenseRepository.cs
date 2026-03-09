using Spendly.Application.Interfaces;
using Spendly.Domain.Entities;
using Spendly.Infrastructure.Persistence;

namespace Spendly.Infrastructure.Repositories
{
    public class RecurringExpenseRepository : IRecurringExpenseRepository
    {
        private readonly SpendlyDbContext _context;

        public RecurringExpenseRepository(SpendlyDbContext context) => _context = context;

        public void Add(RecurringExpense recurringExpense)
        {
            _context.RecurringExpenses.Add(recurringExpense);
            _context.SaveChanges();
        }

        public void Update(RecurringExpense recurringExpense)
        {
            _context.RecurringExpenses.Update(recurringExpense);
            _context.SaveChanges();
        }

        public bool Delete(int id)
        {
            var recurring = _context.RecurringExpenses.Find(id);
            if (recurring == null) return false;

            _context.RecurringExpenses.Remove(recurring);
            _context.SaveChanges();
            return true;
        }

        public RecurringExpense? GetById(int id)
            => _context.RecurringExpenses.FirstOrDefault(r => r.Id == id);

        public List<RecurringExpense> GetAllByUser(int userId)
            => _context.RecurringExpenses
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

        public List<RecurringExpense> GetActiveByUser(int userId)
            => _context.RecurringExpenses
                .Where(r => r.UserId == userId && r.IsActive)
                .ToList();

        public List<RecurringExpense> GetAllDueForGeneration()
        {
            // Obtener todas las recurrencias activas que:
            // 1. Están activas
            // 2. La fecha de inicio ya pasó
            // 3. No tienen fecha de fin O la fecha de fin no ha llegado
            var today = DateTime.Today;

            return _context.RecurringExpenses
                .Where(r => r.IsActive && r.StartDate <= today)
                .Where(r => !r.EndDate.HasValue || r.EndDate.Value >= today)
                .ToList();
        }
    }
}
