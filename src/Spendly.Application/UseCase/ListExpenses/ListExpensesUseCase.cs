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
        private readonly ITagRepository _tagRepository;

        public ListExpensesUseCase(IExpenseRepository expenseRepository, ITagRepository tagRepository)
        {
            _expenseRepository = expenseRepository;
            _tagRepository = tagRepository;
        }

        public async Task<PagedResult<ExpenseResponseDto>> ExecuteAsync(
            int userId,
            string? category,
            string? search,
            DateTime? dateFrom,
            DateTime? dateTo,
            decimal? minAmount,
            decimal? maxAmount,
            int page,
            int pageSize,
            List<int>? tagIds = null)
        {
            var expenses = await _expenseRepository.GetAllAsync(userId, category, search, dateFrom, dateTo, minAmount, maxAmount, page, pageSize, tagIds);
            var total = await _expenseRepository.CountAsync(userId, category, search, dateFrom, dateTo, minAmount, maxAmount, tagIds);

            var expensesList = expenses.ToList();
            var expenseIds = expensesList.Select(e => e.Id).ToList();
            var tagsByExpense = await _tagRepository.GetTagsForExpensesAsync(expenseIds);

            var dtos = expensesList.Select(e =>
            {
                tagsByExpense.TryGetValue(e.Id, out var tags);
                return ExpenseMapper.ToDto(e, tags);
            }).ToList();

            return new PagedResult<ExpenseResponseDto>
            {
                Items = dtos,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}