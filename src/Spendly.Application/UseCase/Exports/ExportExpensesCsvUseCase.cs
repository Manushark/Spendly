using System.Globalization;
using System.Text;
using Spendly.Application.Interfaces;

namespace Spendly.Application.UseCases.Exports
{
    public class ExportExpensesCsvUseCase
    {
        private readonly IExpenseRepository _repo;
        private readonly IDateTimeProvider _dateTime;

        public ExportExpensesCsvUseCase(IExpenseRepository repo, IDateTimeProvider dateTime)
        {
            _repo = repo;
            _dateTime = dateTime;
        }

        public async Task<byte[]> ExecuteAsync(
            int userId,
            string? category = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            string? userTimeZone = null)
        {
            // Get all expenses matching filters (no pagination for export)
            var startDate = dateFrom ?? new DateTime(2000, 1, 1);
            var endDate = dateTo ?? _dateTime.Now(userTimeZone);

            var expenses = (await _repo.GetByDateRangeAsync(userId, startDate, endDate)).ToList();

            // Apply category filter if specified
            if (!string.IsNullOrWhiteSpace(category))
                expenses = expenses.Where(e => e.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("Date,Description,Category,Amount");

            foreach (var expense in expenses.OrderByDescending(e => e.Date))
            {
                var description = EscapeCsv(expense.Description);
                var cat = EscapeCsv(expense.Category);
                sb.AppendLine($"{expense.Date:yyyy-MM-dd},{description},{cat},{expense.Amount.Value.ToString("F2", CultureInfo.InvariantCulture)}");
            }

            return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        }

        private static string EscapeCsv(string value)
        {
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }
    }
}
