using Spendly.Application.DTOs.Expense;
using Spendly.Application.Interfaces;
using Spendly.Application.Mappers;

namespace Spendly.Application.UseCase.GetExpenseById
{
    public class GetExpenseByIdUseCase
    {
        private readonly IExpenseRepository _expenseRepository;

        public GetExpenseByIdUseCase(IExpenseRepository expenseRepository)
        {
            _expenseRepository = expenseRepository;
        }

        public async Task<ExpenseResponseDto?> ExecuteAsync(int userId, int id)
        {
            var expense = await _expenseRepository.GetByIdAsync(id);

            if (expense is null) return null;

            // Verifica que el gasto pertenezca al usuario
            expense.EnsureOwnership(userId);

            return ExpenseMapper.ToDto(expense);
        }
    }
}
