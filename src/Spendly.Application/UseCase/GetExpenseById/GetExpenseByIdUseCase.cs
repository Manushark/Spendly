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

        public ExpenseResponseDto? Execute(int userId, int id)
        {
            var expense = _expenseRepository.GetById(id);

            if (expense is null) return null;

            // Verifica que el gasto pertenezca al usuario
            expense.EnsureOwnership(userId);

            return ExpenseMapper.ToDto(expense);
        }
    }
}
//alternative implementation without mapper
//public ExpenseResponseDto? Execute(int id)
//{
//    var expense = _expenseRepository.GetById(id);

//    if (expense == null)
//        return null;

//    return new ExpenseResponseDto
//    {
//        Id = expense.Id,
//        Amount = expense.Amount.Value,
//        Description = expense.Description,
//        Date = expense.Date,
//        Category = expense.Category
//    };
//}
