namespace Spendly.Application.DTOs.Ai
{
    public class AiExecutionResultDto
    {
        public int CreatedExpensesCount { get; set; }
        public int CreatedBudgetsCount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; } = true;
    }
}
