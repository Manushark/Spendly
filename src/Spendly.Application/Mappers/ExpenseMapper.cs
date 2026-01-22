using Spendly.Application.DTOs.Expense;
using Spendly.Domain.Entities;

namespace Spendly.Application.Mappers
{
    public static class ExpenseMapper
    {
        public static ExpenseResponseDto ToDto(Expense expense)
        {
            return new ExpenseResponseDto
            {
                Id = expense.Id,
                Amount = expense.Amount.Value, // Money → decimal
                Description = expense.Description,
                Date = expense.Date,
                Category = expense.Category
            };
        }
    }
}
