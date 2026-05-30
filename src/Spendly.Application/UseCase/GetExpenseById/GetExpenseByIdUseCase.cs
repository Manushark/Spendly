using Spendly.Application.DTOs.Expense;
using Spendly.Application.Interfaces;
using Spendly.Application.Mappers;

namespace Spendly.Application.UseCase.GetExpenseById
{
    public class GetExpenseByIdUseCase
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly ITagRepository _tagRepository;

        public GetExpenseByIdUseCase(IExpenseRepository expenseRepository, ITagRepository tagRepository)
        {
            _expenseRepository = expenseRepository;
            _tagRepository = tagRepository;
        }

        public async Task<ExpenseResponseDto?> ExecuteAsync(int userId, int id)
        {
            var expense = await _expenseRepository.GetByIdAsync(id);

            if (expense is null) return null;

            // Verifica que el gasto pertenezca al usuario
            expense.EnsureOwnership(userId);

            // Fetch tags for this specific expense
            var tags = await _tagRepository.GetTagsForExpenseAsync(id);

            return ExpenseMapper.ToDto(expense, tags);
        }
    }
}
