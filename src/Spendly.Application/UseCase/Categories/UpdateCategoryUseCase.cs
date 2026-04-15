using Spendly.Application.DTOs.Category;
using Spendly.Application.Interfaces;
using Spendly.Domain.Exceptions;

namespace Spendly.Application.UseCases.Categories
{
    public class UpdateCategoryUseCase
    {
        private readonly ICategoryRepository _repo;

        public UpdateCategoryUseCase(ICategoryRepository repo) => _repo = repo;

        public async Task ExecuteAsync(int userId, int categoryId, UpdateCategoryDto dto)
        {
            var category = await _repo.GetByIdAsync(categoryId)
                ?? throw new InvalidDomainException("Category not found.");

            category.EnsureOwnership(userId);
            category.Update(dto.Name, dto.Icon, dto.Color);
            await _repo.UpdateAsync(category);
        }
    }
}
