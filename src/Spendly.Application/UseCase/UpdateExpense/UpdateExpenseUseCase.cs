using Spendly.Application.DTOs;
using Spendly.Application.DTOs.Expense;
using Spendly.Application.Interfaces;


namespace Spendly.Application.UseCases.Expenses
{
    public class UpdateExpenseUseCase
    {
        private readonly IExpenseRepository _expenseRepository;

        public UpdateExpenseUseCase(IExpenseRepository expenseRepository)
        {
            _expenseRepository = expenseRepository;
        }

        public bool Execute(int id, UpdateExpenseDto dto)
        {
            var expense = _expenseRepository.GetById(id);

            if (expense == null)
                return false;

            expense.Update(
                dto.Amount,
                dto.Description,
                dto.Date,
                dto.Category
            );

            _expenseRepository.Update(expense);

            return true;
        }


    }
}
