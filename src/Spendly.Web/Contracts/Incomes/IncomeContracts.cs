namespace Spendly.Web.Contracts.Incomes
{
    public class IncomeDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Source { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime Date { get; set; }
        public bool IsRecurring { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PagedIncomeResult
    {
        public IEnumerable<IncomeDto> Items { get; set; } = [];
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }

        public static PagedIncomeResult Empty() => new()
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
