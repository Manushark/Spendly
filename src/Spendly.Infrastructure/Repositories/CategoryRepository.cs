using Microsoft.EntityFrameworkCore;
using Spendly.Application.Interfaces;
using Spendly.Domain.Entities;
using Spendly.Infrastructure.Persistence;

namespace Spendly.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly SpendlyDbContext _context;

        public CategoryRepository(SpendlyDbContext context) => _context = context;

        public async Task<List<Category>> GetAllByUserAsync(int userId)
            => await _context.Categories
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.Name)
                .ToListAsync();

        public async Task<Category?> GetByIdAsync(int id)
            => await _context.Categories.FindAsync(id);

        public async Task<Category?> GetByNameAndUserAsync(int userId, string name)
            => await _context.Categories
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Name.ToLower() == name.ToLower());

        public async Task AddAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task SeedDefaultsAsync(int userId)
        {
            var defaults = new List<(string Name, string Icon, string Color)>
            {
                ("Food & Dining",      "bi-cup-hot",       "#FF6B6B"),
                ("Transportation",     "bi-car-front",     "#4ECDC4"),
                ("Entertainment",      "bi-controller",    "#9B59B6"),
                ("Shopping",           "bi-bag",           "#F39C12"),
                ("Health",             "bi-heart-pulse",   "#E74C3C"),
                ("Education",          "bi-book",          "#3498DB"),
                ("Bills & Utilities",  "bi-lightning",     "#1ABC9C"),
                ("Other",              "bi-three-dots",    "#95A5A6")
            };

            foreach (var (name, icon, color) in defaults)
            {
                var category = Category.Create(userId, name, icon, color, isDefault: true);
                _context.Categories.Add(category);
            }

            await _context.SaveChangesAsync();
        }
    }
}
