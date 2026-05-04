using Spendly.Application.DTOs.Category;
using Spendly.Application.Interfaces;
using Spendly.Domain.Exceptions;

namespace Spendly.Application.UseCases.Categories
{
    public class UpdateCategoryUseCase
    {
        private readonly ICategoryRepository _categoryRepo;
        private readonly IExpenseRepository _expenseRepo;
        private readonly IBudgetRepository _budgetRepo;
        private readonly IRecurringExpenseRepository _recurringRepo;

        public UpdateCategoryUseCase(
            ICategoryRepository categoryRepo,
            IExpenseRepository expenseRepo,
            IBudgetRepository budgetRepo,
            IRecurringExpenseRepository recurringRepo)
        {
            _categoryRepo = categoryRepo;
            _expenseRepo = expenseRepo;
            _budgetRepo = budgetRepo;
            _recurringRepo = recurringRepo;
        }

        public async Task ExecuteAsync(int userId, int categoryId, UpdateCategoryDto dto)
        {
            var category = await _categoryRepo.GetByIdAsync(categoryId)
                ?? throw new InvalidDomainException("Category not found.");

            category.EnsureOwnership(userId);

            var oldName = category.Name;
            var nameChanged = !oldName.Equals(dto.Name, StringComparison.OrdinalIgnoreCase);

            category.Update(dto.Name, dto.Icon, dto.Color);
            await _categoryRepo.UpdateAsync(category);

            // Si el nombre cambió, propagar a todos los registros asociados
            if (nameChanged)
            {
                await _expenseRepo.UpdateCategoryNameAsync(userId, oldName, dto.Name);
                await _budgetRepo.UpdateCategoryNameAsync(userId, oldName, dto.Name);
                await _recurringRepo.UpdateCategoryNameAsync(userId, oldName, dto.Name);
            }
        }
    }
}
