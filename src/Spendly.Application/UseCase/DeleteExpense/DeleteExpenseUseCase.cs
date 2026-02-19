using Spendly.Application.Interfaces;
using Spendly.Domain.Exceptions;

namespace Spendly.Application.UseCase.DeleteExpense
{
    public class DeleteExpenseUseCase
    {
        private readonly IExpenseRepository _expenseRepository;

        public DeleteExpenseUseCase(IExpenseRepository expenseRepository)
        {
            _expenseRepository = expenseRepository;
        }

        public bool Execute(int userId, int id)
        {
            var expense = _expenseRepository.GetById(id);

            if (expense is null)
                return false;

            expense.EnsureOwnership(userId);

            return _expenseRepository.Delete(id);
        }
    }
}
