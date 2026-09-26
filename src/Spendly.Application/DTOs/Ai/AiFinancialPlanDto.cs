namespace Spendly.Application.DTOs.Ai
{
    public class AiFinancialPlanDto
    {
        public List<AiParsedExpenseDto> Expenses { get; set; } = [];
        public List<AiParsedBudgetDto> Budgets { get; set; } = [];
        public string Summary { get; set; } = string.Empty;
        public List<string> AvailableCategories { get; set; } = [];
        public string Source { get; set; } = "Gemini";
        public bool HasActions => Expenses.Count > 0 || Budgets.Count > 0;
    }
}
