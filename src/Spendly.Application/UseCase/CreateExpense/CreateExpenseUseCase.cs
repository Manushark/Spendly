using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spendly.Application.DTOs.Expense;
using Spendly.Application.Interfaces;
using Spendly.Domain.Entities;
using Spendly.Domain.ValueObjects;

namespace Spendly.Application.UseCase.CreateExpense
{
    public class CreateExpenseUseCase
    {
        private readonly IExpenseRepository expenseRepository;

        public CreateExpenseUseCase(IExpenseRepository expenseRepository)
        {
            this.expenseRepository = expenseRepository;
        }

        public void Execute(CreateExpenseDto dto)
        {
           
            var expense = new Expense(
              Money.FromDecimal(dto.Amount),
              dto.Description,
              dto.Date,
              dto.Category
            );
            expenseRepository.Add(expense);

        }

    }
}
