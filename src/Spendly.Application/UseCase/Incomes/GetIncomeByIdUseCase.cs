using Spendly.Application.DTOs.Income;
using Spendly.Application.Interfaces;
using Spendly.Application.Mappers;

namespace Spendly.Application.UseCases.Incomes
{
    public class GetIncomeByIdUseCase
    {
        private readonly IIncomeRepository _repo;

        public GetIncomeByIdUseCase(IIncomeRepository repo) => _repo = repo;

        public async Task<IncomeResponseDto?> ExecuteAsync(int userId, int incomeId)
        {
            var income = await _repo.GetByIdAsync(incomeId);
            if (income == null) return null;

            income.EnsureOwnership(userId);
            return IncomeMapper.ToDto(income);
        }
    }
}
