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

        public CreateExpenseUseCase(IExpenseRepository expenseRepository, BudgetAlertService budgetAlertService)
        {
            _expenseRepository = expenseRepository;
            _budgetAlertService = budgetAlertService;
        }

        public async Task<int> ExecuteAsync(int userId, CreateExpenseDto dto)
        {
            // Create the expense entity
            var expense = Expense.Create(
                userId,
                Money.FromDecimal(dto.Amount),
                dto.Description,
                dto.Date,
                dto.Category
            );

            // Persist the expense
            await _expenseRepository.AddAsync(expense);

            // Check budget alerts after creating expense
            await _budgetAlertService.CheckAndCreateAlertsAsync(userId);

            return expense.Id;
        }
    }
}