namespace Spendly.Application.DTOs.Ai
{
    public class AiParsedExpenseDto
    {
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public List<string> Tags { get; set; } = [];
    }
}
