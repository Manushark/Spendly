namespace Spendly.Application.DTOs.Ai
{
    public class AiParsedBudgetDto
    {
        public string Category { get; set; } = string.Empty;
        public decimal MonthlyLimit { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
    }
}
