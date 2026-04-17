using Spendly.Application.DTOs.Income;
using Spendly.Application.Interfaces;
using Spendly.Application.Mappers;
using Spendly.Application.UseCase.ListExpenses;

namespace Spendly.Application.UseCases.Incomes
{
    public class ListIncomesUseCase
    {
        private readonly IIncomeRepository _repo;

        public ListIncomesUseCase(IIncomeRepository repo) => _repo = repo;

        public async Task<PagedResult<IncomeResponseDto>> ExecuteAsync(int userId, int page, int pageSize)
        {
            var incomes = await _repo.GetAllByUserAsync(userId, page, pageSize);
            var total = await _repo.CountByUserAsync(userId);

            return new PagedResult<IncomeResponseDto>
            {
                Items = incomes.Select(IncomeMapper.ToDto),
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
