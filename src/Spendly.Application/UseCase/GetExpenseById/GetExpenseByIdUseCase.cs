using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spendly.Application.DTOs.Expense;
using Spendly.Application.Interfaces;

namespace Spendly.Application.UseCase.GetExpenseById
{
    public class GetExpenseByIdUseCase
    {
        private readonly IExpenseRepository _expenseRepository;

        public GetExpenseByIdUseCase(IExpenseRepository expenseRepository)
        {
            _expenseRepository = expenseRepository;
        }

        public ExpenseResponseDto? Execute(int id)
        {
            var expense = _expenseRepository.GetById(id);

            if (expense == null)
                return null;

            return new ExpenseResponseDto
            {
                Id = expense.Id,
                Amount = expense.Amount.Value,
                Description = expense.Description,
                Date = expense.Date,
                Category = expense.Category
            };
        }
    }
}
