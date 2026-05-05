using Spendly.Application.DTOs.Category;
using Spendly.Application.Interfaces;
using Spendly.Domain.Entities;
using Spendly.Domain.Exceptions;

namespace Spendly.Application.UseCases.Categories
{
    public class CreateCategoryUseCase
    {
        private readonly ICategoryRepository _repo;

        public CreateCategoryUseCase(ICategoryRepository repo) => _repo = repo;

        public async Task<int> ExecuteAsync(int userId, CreateCategoryDto dto)
        {
            // Check category limit per user
            var count = await _repo.CountByUserAsync(userId);
            if (count >= 50)
                throw new InvalidDomainException("You cannot have more than 50 categories. Please delete an existing category first.");

            // Check for duplicate name
            var existing = await _repo.GetByNameAndUserAsync(userId, dto.Name);
            if (existing != null)
                throw new InvalidDomainException($"A category named '{dto.Name}' already exists.");

            var category = Category.Create(userId, dto.Name, dto.Icon, dto.Color);
            await _repo.AddAsync(category);
            return category.Id;
        }
    }
}
