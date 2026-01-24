namespace Spendly.Application.DTOs.Expense
{
    public class ListExpensesQueryDto
    {
        public string? Category { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
