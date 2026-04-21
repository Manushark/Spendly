using Spendly.Application.DTOs.Income;
using Spendly.Application.Interfaces;
using Spendly.Domain.Entities;

namespace Spendly.Application.UseCases.Incomes
{
    public class CreateIncomeUseCase
    {
        private readonly IIncomeRepository _repo;

        public CreateIncomeUseCase(IIncomeRepository repo) => _repo = repo;

        public async Task<int> ExecuteAsync(int userId, CreateIncomeDto dto)
        {
            var income = Income.Create(
                userId,
                dto.Amount,
                dto.Currency,
                dto.Source,
                dto.Description,
                dto.Date,
                dto.IsRecurring
            );

            await _repo.AddAsync(income);
            return income.Id;
        }
    }
}
