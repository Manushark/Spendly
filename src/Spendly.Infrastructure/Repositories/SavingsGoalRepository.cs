using Microsoft.EntityFrameworkCore;
using Spendly.Application.Interfaces;
using Spendly.Domain.Entities;
using Spendly.Infrastructure.Persistence;

namespace Spendly.Infrastructure.Repositories
{
    public class SavingsGoalRepository : ISavingsGoalRepository
    {
        private readonly SpendlyDbContext _context;

        public SavingsGoalRepository(SpendlyDbContext context) => _context = context;

        public async Task AddAsync(SavingsGoal goal)
        {
            _context.SavingsGoals.Add(goal);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(SavingsGoal goal)
        {
            _context.SavingsGoals.Update(goal);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var goal = await _context.SavingsGoals.FindAsync(id);
            if (goal == null) return false;
            _context.SavingsGoals.Remove(goal);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<SavingsGoal?> GetByIdAsync(int id)
        {
            return await _context.SavingsGoals.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<SavingsGoal>> GetAllByUserAsync(int userId)
        {
            return await _context.SavingsGoals
                .Where(s => s.UserId == userId)
                .OrderBy(s => s.IsCompleted)
                .ThenByDescending(s => s.CreatedAt)
                .ToListAsync();
        }
    }
}
