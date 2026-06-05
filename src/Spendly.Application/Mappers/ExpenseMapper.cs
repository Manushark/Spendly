using Spendly.Application.DTOs.Expense;
using Spendly.Application.DTOs.Tag;
using Spendly.Domain.Entities;

namespace Spendly.Application.Mappers
{
    public static class ExpenseMapper
    {
        public static ExpenseResponseDto ToDto(Expense expense)
        {
            return ToDto(expense, null);
        }

        public static ExpenseResponseDto ToDto(Expense expense, List<Tag>? tags)
        {
            return new ExpenseResponseDto
            {
                Id = expense.Id,
                Amount = expense.Amount.Value, // Money → decimal
                Description = expense.Description,
                Date = expense.Date,
                Category = expense.Category,
                Tags = tags?.Select(t => new TagResponseDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Color = t.Color
                }).ToList() ?? []
            };
        }
    }
}
