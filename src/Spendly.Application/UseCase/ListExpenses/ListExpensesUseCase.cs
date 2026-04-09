using Spendly.Application.DTOs.Expense;
using Spendly.Application.Interfaces;
using Spendly.Application.Mappers;

namespace Spendly.Application.UseCase.ListExpenses
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = [];
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }

    public class ListExpensesUseCase
    {
        private readonly IExpenseRepository _expenseRepository;

        public ListExpensesUseCase(IExpenseRepository expenseRepository)
        {
            _expenseRepository = expenseRepository;
        }

        public async Task<PagedResult<ExpenseResponseDto>> ExecuteAsync(int userId, string? category, int page, int pageSize)
        {
            var expenses = await _expenseRepository.GetAllAsync(userId, category, page, pageSize);
            var total = await _expenseRepository.CountAsync(userId, category);

            return new PagedResult<ExpenseResponseDto>
            {
                Items = expenses.Select(ExpenseMapper.ToDto),
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}