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

        public async Task<bool> ExecuteAsync(int userId, int id)
        {
            var expense = await _expenseRepository.GetByIdAsync(id);

            if (expense is null)
                return false;

            expense.EnsureOwnership(userId);

            return await _expenseRepository.DeleteAsync(id);
        }
    }
}
