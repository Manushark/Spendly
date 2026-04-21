using Spendly.Application.DTOs.Income;
using Spendly.Application.Interfaces;

namespace Spendly.Application.UseCases.Incomes
{
    public class UpdateIncomeUseCase
    {
        private readonly IIncomeRepository _repo;

        public UpdateIncomeUseCase(IIncomeRepository repo) => _repo = repo;

        public async Task ExecuteAsync(int userId, int incomeId, UpdateIncomeDto dto)
        {
            var income = await _repo.GetByIdAsync(incomeId)
                ?? throw new KeyNotFoundException($"Income {incomeId} not found.");

            income.EnsureOwnership(userId);
            income.Update(dto.Amount, dto.Currency, dto.Source, dto.Description, dto.Date, dto.IsRecurring);
            await _repo.UpdateAsync(income);
        }
    }
}
