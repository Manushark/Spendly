using Spendly.Application.DTOs.Expense;
using Spendly.Application.Interfaces;
using Spendly.Domain.Entities;
using Spendly.Domain.ValueObjects;

namespace Spendly.Application.UseCase.CreateExpense
{
    /// <summary>
    /// Use case for creating a new expense.
    /// Implements the business logic for expense creation.
    /// </summary>
    public class CreateExpenseUseCase
    {
        private readonly IExpenseRepository _expenseRepository;

        public CreateExpenseUseCase(IExpenseRepository expenseRepository)
        {
            _expenseRepository = expenseRepository;
        }

        /// <summary>
        /// Executes the use case to create a new expense.
        /// </summary>
        /// <param name="dto">The expense data transfer object</param>
        public void Execute(CreateExpenseDto dto)
        {
            // Create the expense entity
            var expense = Expense.Create(
                Money.FromDecimal(dto.Amount),
                dto.Description,
                dto.Date,
                dto.Category
            );

            // Persist the expense
            _expenseRepository.Add(expense);
        }
    }
}