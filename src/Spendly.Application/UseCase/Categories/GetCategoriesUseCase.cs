using Spendly.Application.DTOs.Category;
using Spendly.Application.Interfaces;

namespace Spendly.Application.UseCases.Categories
{
    public class GetCategoriesUseCase
    {
        private readonly ICategoryRepository _repo;

        public GetCategoriesUseCase(ICategoryRepository repo) => _repo = repo;

        public async Task<List<CategoryDto>> ExecuteAsync(int userId)
        {
            var categories = await _repo.GetAllByUserAsync(userId);

            return categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Icon = c.Icon,
                Color = c.Color,
                IsDefault = c.IsDefault
            }).ToList();
        }
    }
}
