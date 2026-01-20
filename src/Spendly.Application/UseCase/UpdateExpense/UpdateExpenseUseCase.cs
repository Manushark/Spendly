using Spendly.Application.DTOs.Expense;
using Spendly.Application.Interfaces;
using Spendly.Domain.Exceptions;
using Spendly.Domain.ValueObjects;

namespace Spendly.Application.UseCases.Expenses
{
    public class UpdateExpenseUseCase
    {
        private readonly IExpenseRepository _expenseRepository;

        public UpdateExpenseUseCase(IExpenseRepository expenseRepository)
        {
            _expenseRepository = expenseRepository;
        }

        public void Execute(int id, UpdateExpenseDto dto)
        {
            var expense = _expenseRepository.GetById(id);

            if (expense is null)
                throw new ExpenseNotFoundException(id);

            var money = Money.FromDecimal(dto.Amount); 
            expense.Update(
                money,
                dto.Description,
                dto.Date,
                dto.Category
            );

            _expenseRepository.Update(expense);
        }
    }
}
