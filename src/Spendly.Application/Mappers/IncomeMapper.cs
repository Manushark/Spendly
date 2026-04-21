using Spendly.Application.DTOs.Income;
using Spendly.Domain.Entities;

namespace Spendly.Application.Mappers
{
    public static class IncomeMapper
    {
        public static IncomeResponseDto ToDto(Income income)
        {
            return new IncomeResponseDto
            {
                Id = income.Id,
                Amount = income.Amount.Value,
                Currency = income.Currency,
                Source = income.Source,
                Description = income.Description,
                Date = income.Date,
                IsRecurring = income.IsRecurring,
                CreatedAt = income.CreatedAt
            };
        }
    }
}
