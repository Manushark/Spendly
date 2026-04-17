using Spendly.Application.Interfaces;

namespace Spendly.Application.UseCases.Incomes
{
    public class DeleteIncomeUseCase
    {
        private readonly IIncomeRepository _repo;

        public DeleteIncomeUseCase(IIncomeRepository repo) => _repo = repo;

        public async Task<bool> ExecuteAsync(int userId, int incomeId)
        {
            var income = await _repo.GetByIdAsync(incomeId);
            if (income == null) return false;

            income.EnsureOwnership(userId);
            return await _repo.DeleteAsync(incomeId);
        }
    }
}
