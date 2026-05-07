using Microsoft.EntityFrameworkCore;
using Spendly.Application.Interfaces;
using Spendly.Domain.Entities;
using Spendly.Infrastructure.Persistence;

namespace Spendly.Infrastructure.Repositories
{
    public class TagRepository : ITagRepository
    {
        private readonly SpendlyDbContext _context;

        public TagRepository(SpendlyDbContext context) => _context = context;

        public async Task AddAsync(Tag tag)
        {
            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Tag tag)
        {
            _context.Tags.Update(tag);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null) return false;

            // Remove all expense-tag relationships first
            var expenseTags = await _context.ExpenseTags.Where(et => et.TagId == id).ToListAsync();
            _context.ExpenseTags.RemoveRange(expenseTags);

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Tag?> GetByIdAsync(int id)
        {
            return await _context.Tags
                .Include(t => t.ExpenseTags)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<Tag>> GetAllByUserAsync(int userId)
        {
            return await _context.Tags
                .Include(t => t.ExpenseTags)
                .Where(t => t.UserId == userId)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<Tag?> GetByNameAsync(int userId, string name)
        {
            return await _context.Tags
                .FirstOrDefaultAsync(t => t.UserId == userId && t.Name == name);
        }

        public async Task SetExpenseTagsAsync(int userId, int expenseId, List<int> tagIds)
        {
            var expenseExists = await _context.Expenses
                .AnyAsync(e => e.Id == expenseId && e.UserId == userId);

            if (!expenseExists)
                throw new KeyNotFoundException("Expense not found.");

            var distinctTagIds = tagIds
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            if (distinctTagIds.Count > 0)
            {
                var ownedTagIds = await _context.Tags
                    .Where(t => t.UserId == userId && distinctTagIds.Contains(t.Id))
                    .Select(t => t.Id)
                    .ToListAsync();

                if (ownedTagIds.Count != distinctTagIds.Count)
                    throw new InvalidOperationException("One or more tags do not belong to the authenticated user.");
            }

            // Remove existing
            var existing = await _context.ExpenseTags
                .Where(et => et.ExpenseId == expenseId)
                .ToListAsync();
            _context.ExpenseTags.RemoveRange(existing);

            // Add new
            foreach (var tagId in distinctTagIds)
            {
                _context.ExpenseTags.Add(new ExpenseTag { ExpenseId = expenseId, TagId = tagId });
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<Tag>> GetTagsForExpenseAsync(int expenseId)
        {
            return await _context.ExpenseTags
                .Where(et => et.ExpenseId == expenseId)
                .Select(et => et.Tag)
                .ToListAsync();
        }

        public async Task<Dictionary<int, List<Tag>>> GetTagsForExpensesAsync(List<int> expenseIds)
        {
            var data = await _context.ExpenseTags
                .Where(et => expenseIds.Contains(et.ExpenseId))
                .Include(et => et.Tag)
                .ToListAsync();

            return data
                .GroupBy(et => et.ExpenseId)
                .ToDictionary(g => g.Key, g => g.Select(et => et.Tag).ToList());
        }
    }
}
