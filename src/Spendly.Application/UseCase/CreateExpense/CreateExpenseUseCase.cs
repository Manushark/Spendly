using Spendly.Application.DTOs.Expense;
using Spendly.Application.Interfaces;
using Spendly.Domain.Entities;
using Spendly.Domain.ValueObjects;

namespace Spendly.Application.UseCase.CreateExpense
{
    public class CreateExpenseUseCase
    {
        private readonly IExpenseRepository _expenseRepository;

        public CreateExpenseUseCase(IExpenseRepository expenseRepository)
        {
            _expenseRepository = expenseRepository;
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

            return expense.Id;
        }
    }
}