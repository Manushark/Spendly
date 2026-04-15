using Spendly.Application.Interfaces;
using Spendly.Domain.Exceptions;

namespace Spendly.Application.UseCases.Categories
{
    public class DeleteCategoryUseCase
    {
        private readonly ICategoryRepository _repo;

        public DeleteCategoryUseCase(ICategoryRepository repo) => _repo = repo;

        public async Task<bool> ExecuteAsync(int userId, int categoryId)
        {
            var category = await _repo.GetByIdAsync(categoryId)
                ?? throw new InvalidDomainException("Category not found.");

            category.EnsureOwnership(userId);

            return await _repo.DeleteAsync(categoryId);
        }
    }
}
