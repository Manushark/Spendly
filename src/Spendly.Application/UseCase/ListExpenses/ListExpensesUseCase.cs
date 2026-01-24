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
//alternative implementation without mapper
//public List<ExpenseResponseDto> Execute()
//{
//    var expenses = _expenseRepository.GetAll();
//    return expenses.Select(e => new ExpenseResponseDto
//    {
//        Id = e.Id,
//        Amount = e.Amount.Value,
//        Description = e.Description,
//        Date = e.Date,
//        Category = e.Category
//    }).ToList();
//}