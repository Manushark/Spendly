namespace Spendly.Application.DTOs.Import
{
    public class CsvImportPreviewDto
    {
        public List<CsvExpenseRow> Rows { get; set; } = [];
        public int TotalRows { get; set; }
        public int ValidRows { get; set; }
        public int InvalidRows { get; set; }
        public List<string> Errors { get; set; } = [];
        public List<string> DetectedColumns { get; set; } = [];
    }

    public class CsvExpenseRow
    {
        public int RowNumber { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Currency { get; set; } = "USD";
        public bool IsValid { get; set; } = true;
        public string? ValidationError { get; set; }
        public string? CategoryWarning { get; set; }
    }

    public class CsvImportResultDto
    {
        public int ImportedCount { get; set; }
        public int SkippedCount { get; set; }
        public List<string> Errors { get; set; } = [];
    }
}
