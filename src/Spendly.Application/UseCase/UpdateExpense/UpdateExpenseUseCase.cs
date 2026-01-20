using Spendly.Application.DTOs.Expense;
using Spendly.Application.Interfaces;
using Spendly.Domain.Exceptions;

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

            expense.Update(
                dto.Amount,
                dto.Description,
                dto.Date,
                dto.Category
            );

            _expenseRepository.Update(expense);
        }
    }
}
