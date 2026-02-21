namespace Spendly.Web.Contracts.Expenses
{
    public class PagedExpenseResult
    {
        public IEnumerable<ExpenseDto> Items { get; set; } = [];
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }

        public static PagedExpenseResult Empty() => new()
        {
            Items = [],
            TotalCount = 0,
            Page = 1,
            PageSize = 10,
            TotalPages = 0,
            HasPreviousPage = false,
            HasNextPage = false
        };
    }
}