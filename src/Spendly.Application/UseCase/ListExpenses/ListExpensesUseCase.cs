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

        public PagedResult<ExpenseResponseDto> Execute(int userId, string? category, int page, int pageSize)
        {
            var expenses = _expenseRepository.GetAll(userId, category, page, pageSize);
            var total = _expenseRepository.Count(userId, category);

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
//alternative implementation without mapper
//public List<ExpenseResponseDto> Execute()
//{
//    var expenses = _expenseRepository.GetAll();
//    return expenses.Select(e => new ExpenseResponseDto
//    {
//        Id = e.Id,
//        Amount = e.Amount.Value,
//        Description = e.Description,
//        Date = e.Date,
//        Category = e.Category
//    }).ToList();
//}