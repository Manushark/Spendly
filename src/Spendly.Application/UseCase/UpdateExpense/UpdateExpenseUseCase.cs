using Spendly.Application.DTOs.Expense;
using Spendly.Application.Interfaces;
using Spendly.Application.Services;
using Spendly.Domain.Exceptions;
using Spendly.Domain.ValueObjects;

namespace Spendly.Application.UseCases.Expenses
{
    public class UpdateExpenseUseCase
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly BudgetAlertService _budgetAlertService;
        private readonly ITagRepository _tagRepository;

        public UpdateExpenseUseCase(
            IExpenseRepository expenseRepository,
            BudgetAlertService budgetAlertService,
            ITagRepository tagRepository)
        {
            _expenseRepository = expenseRepository;
            _budgetAlertService = budgetAlertService;
            _tagRepository = tagRepository;
        }

        public async Task ExecuteAsync(int userId, int id, UpdateExpenseDto dto)
        {
            var expense = await _expenseRepository.GetByIdAsync(id);

            if (expense is null)
                throw new ExpenseNotFoundException(id);

            // Verifica que el gasto pertenezca al usuario antes de modificar
            expense.EnsureOwnership(userId);

            expense.Update(
                Money.Create(dto.Amount, dto.Currency),
                dto.Description,
                dto.Date,
                dto.Category
            );

            await _expenseRepository.UpdateAsync(expense);

            // Update tags for the expense
            await _tagRepository.SetExpenseTagsAsync(userId, expense.Id, dto.TagIds ?? []);

            // Re-evaluate budget alerts after modification — the new amount
            // may push the user over a budget limit that was not exceeded before.
            await _budgetAlertService.CheckAndCreateAlertsAsync(userId);
        }
    }
}
