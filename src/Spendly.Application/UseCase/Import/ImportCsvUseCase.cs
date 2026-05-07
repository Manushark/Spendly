using System.Globalization;
using Spendly.Application.DTOs.Import;
using Spendly.Application.Interfaces;
using Spendly.Domain.Entities;
using Spendly.Domain.Exceptions;
using Spendly.Domain.ValueObjects;

namespace Spendly.Application.UseCases.Import
{
    public class ImportCsvUseCase
    {
        public const int MaxRowsPerImport = 250;
        private readonly IExpenseRepository _expenseRepo;
        private readonly ICategoryRepository _categoryRepo;

        public ImportCsvUseCase(IExpenseRepository expenseRepo, ICategoryRepository categoryRepo)
        {
            _expenseRepo = expenseRepo;
            _categoryRepo = categoryRepo;
        }

        /// <summary>
        /// Preview CSV data before importing — validates rows without persisting.
        /// </summary>
        public CsvImportPreviewDto Preview(string csvContent, string defaultCurrency = "USD")
        {
            var result = new CsvImportPreviewDto();
            var lines = csvContent.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length < 2)
            {
                result.Errors.Add("CSV must have at least a header row and one data row.");
                return result;
            }

            if (lines.Length - 1 > MaxRowsPerImport)
            {
                result.Errors.Add($"CSV exceeds the maximum of {MaxRowsPerImport} rows per import.");
                return result;
            }

            // Parse header
            var header = lines[0].Split(',').Select(h => h.Trim().ToLowerInvariant().Trim('"')).ToArray();
            result.DetectedColumns = header.ToList();

            var amountIdx = FindColumnIndex(header, "amount", "monto", "valor", "total");
            var descIdx = FindColumnIndex(header, "description", "desc", "descripcion", "concepto", "note", "memo");
            var catIdx = FindColumnIndex(header, "category", "categoria", "type", "tipo");
            var dateIdx = FindColumnIndex(header, "date", "fecha", "transaction date", "fecha de transaccion");
            var currIdx = FindColumnIndex(header, "currency", "moneda", "divisa");

            if (amountIdx < 0)
            {
                result.Errors.Add("Could not find 'Amount' column. Expected: amount, monto, valor, total");
                return result;
            }
            if (dateIdx < 0)
            {
                result.Errors.Add("Could not find 'Date' column. Expected: date, fecha, transaction date");
                return result;
            }

            // Parse rows
            for (int i = 1; i < lines.Length; i++)
            {
                var cols = ParseCsvLine(lines[i]);
                var row = new CsvExpenseRow { RowNumber = i + 1 };

                // Amount
                if (amountIdx < cols.Length && decimal.TryParse(cols[amountIdx].Trim('"', ' ', '$', '€'),
                    NumberStyles.Any, CultureInfo.InvariantCulture, out var amt) && amt > 0)
                {
                    row.Amount = Math.Abs(amt);
                }
                else
                {
                    row.IsValid = false;
                    row.ValidationError = "Invalid amount";
                }

                // Description
                row.Description = descIdx >= 0 && descIdx < cols.Length
                    ? cols[descIdx].Trim('"', ' ')
                    : "Imported expense";

                if (string.IsNullOrWhiteSpace(row.Description))
                    row.Description = "Imported expense";

                if (row.Description.Length > 200)
                {
                    row.IsValid = false;
                    row.ValidationError = $"Description exceeds 200 characters (row {row.RowNumber})";
                }

                // Category
                row.Category = catIdx >= 0 && catIdx < cols.Length && !string.IsNullOrWhiteSpace(cols[catIdx].Trim('"', ' '))
                    ? cols[catIdx].Trim('"', ' ')
                    : "Other";

                if (row.Category.Length > 100)
                {
                    row.IsValid = false;
                    row.ValidationError = $"Category exceeds 100 characters (row {row.RowNumber})";
                }

                // Date
                if (dateIdx < cols.Length)
                {
                    var dateStr = cols[dateIdx].Trim('"', ' ');
                    if (DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                    {
                        if (dt > DateTime.UtcNow)
                        {
                            row.IsValid = false;
                            row.ValidationError = $"Future date not allowed: {dateStr} (row {row.RowNumber})";
                        }
                        else
                        {
                            row.Date = dt;
                        }
                    }
                    else if (DateTime.TryParseExact(dateStr, new[] { "dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-dd", "d/M/yyyy" },
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                    {
                        if (dt > DateTime.UtcNow)
                        {
                            row.IsValid = false;
                            row.ValidationError = $"Future date not allowed: {dateStr} (row {row.RowNumber})";
                        }
                        else
                        {
                            row.Date = dt;
                        }
                    }
                    else
                    {
                        row.IsValid = false;
                        row.ValidationError = $"Unrecognized date format: '{dateStr}' (row {row.RowNumber}). Expected formats: yyyy-MM-dd, MM/dd/yyyy, dd/MM/yyyy";
                    }
                }
                else
                {
                    row.IsValid = false;
                    row.ValidationError = $"Missing date (row {row.RowNumber})";
                }

                // Currency
                row.Currency = currIdx >= 0 && currIdx < cols.Length && !string.IsNullOrWhiteSpace(cols[currIdx].Trim('"', ' '))
                    ? cols[currIdx].Trim('"', ' ').ToUpperInvariant()
                    : defaultCurrency;

                if (row.Currency.Length > 10)
                {
                    row.IsValid = false;
                    row.ValidationError = $"Currency exceeds 10 characters (row {row.RowNumber})";
                }

                result.Rows.Add(row);
            }

            result.TotalRows = result.Rows.Count;
            result.ValidRows = result.Rows.Count(r => r.IsValid);
            result.InvalidRows = result.Rows.Count(r => !r.IsValid);

            return result;
        }

        /// <summary>
        /// Validates CSV row categories against the user's category catalogue.
        /// Adds warnings to rows whose categories don't exist.
        /// </summary>
        public async Task ValidateCategoriesAsync(CsvImportPreviewDto preview, int userId)
        {
            var userCategories = await _categoryRepo.GetAllByUserAsync(userId);
            var categoryNames = userCategories
                .Select(c => c.Name.ToLowerInvariant())
                .ToHashSet();

            foreach (var row in preview.Rows.Where(r => r.IsValid))
            {
                if (!categoryNames.Contains(row.Category.ToLowerInvariant()))
                {
                    row.CategoryWarning = $"Category '{row.Category}' does not exist in your catalogue. It will be created as text only.";
                }
            }
        }

        /// <summary>
        /// Import validated CSV rows into the database.
        /// </summary>
        public async Task<CsvImportResultDto> ImportAsync(int userId, List<CsvExpenseRow> rows)
        {
            if (rows.Count > MaxRowsPerImport)
                throw new InvalidDomainException($"A single import cannot exceed {MaxRowsPerImport} rows.");

            var result = new CsvImportResultDto();
            var validRows = rows.Where(r => r.IsValid && r.Amount > 0).ToList();

            foreach (var row in validRows)
            {
                try
                {
                    var expense = Expense.Create(
                        userId,
                        Money.Create(row.Amount, row.Currency),
                        row.Description,
                        row.Date > DateTime.UtcNow ? DateTime.UtcNow : row.Date,
                        row.Category
                    );
                    await _expenseRepo.AddAsync(expense);
                    result.ImportedCount++;
                }
                catch (Exception ex)
                {
                    result.SkippedCount++;
                    result.Errors.Add($"Row {row.RowNumber}: {ex.Message}");
                }
            }

            result.SkippedCount += rows.Count(r => !r.IsValid);

            return result;
        }

        private static int FindColumnIndex(string[] header, params string[] names)
        {
            for (int i = 0; i < header.Length; i++)
            {
                if (names.Any(n => header[i].Contains(n, StringComparison.OrdinalIgnoreCase)))
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Simple CSV line parser that handles quoted fields with commas.
        /// </summary>
        private static string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            var current = new System.Text.StringBuilder();

            foreach (char c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            result.Add(current.ToString());
            return result.ToArray();
        }
    }
}
