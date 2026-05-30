using Spendly.Application.DTOs.Expense;
using Spendly.Application.Interfaces;
using Spendly.Application.Services;
using Spendly.Domain.Entities;
using Spendly.Domain.ValueObjects;

namespace Spendly.Application.UseCase.CreateExpense
{
    public class CreateExpenseUseCase
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly BudgetAlertService _budgetAlertService;
        private readonly ITagRepository _tagRepository;

        public CreateExpenseUseCase(
            IExpenseRepository expenseRepository, 
            BudgetAlertService budgetAlertService,
            ITagRepository tagRepository)
        {
            _expenseRepository = expenseRepository;
            _budgetAlertService = budgetAlertService;
            _tagRepository = tagRepository;
        }

        public async Task<int> ExecuteAsync(int userId, CreateExpenseDto dto)
        {
            // Create the expense entity
            var expense = Expense.Create(
                userId,
                Money.Create(dto.Amount, dto.Currency),
                dto.Description,
                dto.Date,
                dto.Category
            );

            // Persist the expense
            await _expenseRepository.AddAsync(expense);

            // Set tags for the expense
            if (dto.TagIds != null && dto.TagIds.Any())
            {
                await _tagRepository.SetExpenseTagsAsync(userId, expense.Id, dto.TagIds);
            }

            // Check budget alerts after creating expense
            await _budgetAlertService.CheckAndCreateAlertsAsync(userId);

            return expense.Id;
        }
    }
}