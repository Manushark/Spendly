using Spendly.Application.Interfaces;
using Spendly.Domain.Exceptions;

namespace Spendly.Application.UseCases.Categories
{
    public class DeleteCategoryUseCase
    {
        private readonly ICategoryRepository _categoryRepo;
        private readonly IExpenseRepository _expenseRepo;
        private readonly IBudgetRepository _budgetRepo;
        private readonly IRecurringExpenseRepository _recurringRepo;

        public DeleteCategoryUseCase(
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

        public async Task<bool> ExecuteAsync(int userId, int categoryId)
        {
            var category = await _categoryRepo.GetByIdAsync(categoryId)
                ?? throw new InvalidDomainException("Category not found.");

            category.EnsureOwnership(userId);

            // Verificar si hay registros asociados a esta categoría
            var expenseCount = await _expenseRepo.CountByCategoryAsync(userId, category.Name);
            var budgetCount = await _budgetRepo.CountByCategoryAsync(userId, category.Name);
            var recurringCount = await _recurringRepo.CountByCategoryAsync(userId, category.Name);

            var totalAssociated = expenseCount + budgetCount + recurringCount;

            if (totalAssociated > 0)
            {
                var details = new List<string>();
                if (expenseCount > 0) details.Add($"{expenseCount} expense(s)");
                if (budgetCount > 0) details.Add($"{budgetCount} budget(s)");
                if (recurringCount > 0) details.Add($"{recurringCount} recurring expense(s)");

                throw new InvalidDomainException(
                    $"Cannot delete category '{category.Name}' because it has {string.Join(", ", details)} associated. " +
                    $"Reassign them to another category first.");
            }

            return await _categoryRepo.DeleteAsync(categoryId);
        }
    }
}
