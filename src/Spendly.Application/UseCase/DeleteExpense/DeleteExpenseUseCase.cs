using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spendly.Application.Interfaces;

namespace Spendly.Application.UseCase.DeleteExpense
{
    public class DeleteExpenseUseCase
    {
        private readonly IExpenseRepository _expenseRepository;
        public DeleteExpenseUseCase(IExpenseRepository expenseRepository)
        {
            _expenseRepository = expenseRepository;
        }

        public bool Execute(int id)
        {
            return _expenseRepository.Delete(id);
        }
    }
}
