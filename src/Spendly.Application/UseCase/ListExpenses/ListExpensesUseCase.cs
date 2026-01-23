using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spendly.Application.DTOs.Expense;
using Spendly.Application.Interfaces;
using Spendly.Application.Mappers;
using Spendly.Domain.Entities;

namespace Spendly.Application.UseCase.ListExpenses
{
    public class ListExpensesUseCase  
    {
        private readonly IExpenseRepository _expenseRepository;

        public ListExpensesUseCase(IExpenseRepository expenseRepository)
        {
            _expenseRepository = expenseRepository;
        }

        // Retrieves all expenses and maps them to ExpenseResponseDto
        public IEnumerable<ExpenseResponseDto> Execute()
        {
            var expenses = _expenseRepository.GetAll();

            return expenses.Select(ExpenseMapper.ToDto);
        }
    }
}
