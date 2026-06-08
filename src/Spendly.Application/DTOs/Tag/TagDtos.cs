namespace Spendly.Application.DTOs.Tag
{
    public class CreateTagDto
    {
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = "#6366f1";
    }

    public class UpdateTagDto
    {
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }

    public class TagResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int ExpenseCount { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class TagExpenseDto
    {
        public List<int> TagIds { get; set; } = [];
    }
}
