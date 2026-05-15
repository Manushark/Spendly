using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Spendly.Web.Contracts.Reports;
using System.Text;

namespace Spendly.Web.Services
{
    /// <summary>
    /// Generates PDF (QuestPDF Community) and CSV export files from a FinancialReportDto.
    /// </summary>
    public class ReportExportService
    {
        // ── PDF ────────────────────────────────────────────────────────
        public byte[] GeneratePdf(FinancialReportDto report)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.DefaultTextStyle(ts => ts.FontFamily("Arial").FontSize(10).FontColor("#212529"));

                    page.Header().Element(c => ComposeHeader(c, report));
                    page.Content().PaddingTop(16).Column(col =>
                    {
                        col.Spacing(18);
                        col.Item().Element(c => ComposeKpiRow(c, report));
                        col.Item().Element(c => ComposeTrendTable(c, report));
                        col.Item().Element(c => ComposeCategoryTable(c, report));
                    });
                    page.Footer().Element(ComposeFooter);
                });
            }).GeneratePdf();
        }

        // ── Header ─────────────────────────────────────────────────────
        private static void ComposeHeader(IContainer container, FinancialReportDto report)
        {
            container
                .BorderBottom(1).BorderColor("#dee2e6")
                .PaddingBottom(12)
                .Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Spendly").Bold().FontSize(20).FontColor("#0d6efd");
                        col.Item().Text("Financial Report").FontSize(12).FontColor("#6c757d");
                    });

                    row.ConstantItem(180).AlignRight().Column(col =>
                    {
                        col.Item().Text(report.PeriodLabel).Bold().FontSize(14);
                        col.Item().Text($"Generated: {DateTime.Now:dd MMM yyyy}").FontSize(9).FontColor("#6c757d");
                    });
                });
        }

        // ── KPI Row ────────────────────────────────────────────────────
        private static void ComposeKpiRow(IContainer container, FinancialReportDto report)
        {
            bool profit = report.NetBalance >= 0;

            container.Column(col =>
            {
                col.Item().PaddingBottom(8).Text("Summary").Bold().FontSize(11);

                col.Item().Row(row =>
                {
                    row.RelativeItem().Element(c => KpiCard(c, "Total Expenses",
                        report.TotalExpenses.ToString("N2"), "#dc3545"));
                    row.ConstantItem(6);
                    row.RelativeItem().Element(c => KpiCard(c, "Total Income",
                        report.TotalIncomes.ToString("N2"), "#198754"));
                    row.ConstantItem(6);
                    row.RelativeItem().Element(c => KpiCard(c, "Net Balance",
                        $"{(profit ? "+" : "")}{report.NetBalance:N2}",
                        profit ? "#198754" : "#dc3545"));
                    row.ConstantItem(6);
                    row.RelativeItem().Element(c => KpiCard(c, "Daily Average",
                        report.AverageDailyExpense.ToString("N2"), "#0d6efd"));
                    row.ConstantItem(6);
                    row.RelativeItem().Element(c => KpiCard(c, "Exp. Ratio",
                        $"{report.ExpenseToIncomeRatio:N1}%",
                        report.ExpenseToIncomeRatio > 100 ? "#dc3545" : "#0d6efd"));
                    row.ConstantItem(6);
                    row.RelativeItem().Element(c => KpiCard(c, "Top Category",
                        report.TopCategory, "#6f42c1"));
                });
            });
        }

        private static void KpiCard(IContainer container, string label, string value, string color)
        {
            container
                .Background("#f8f9fa")
                .Border(1).BorderColor("#dee2e6")
                .Padding(10)
                .Column(col =>
                {
                    col.Item().Text(label).FontSize(8).FontColor("#6c757d").Bold();
                    col.Item().PaddingTop(4).Text(value).Bold().FontSize(12).FontColor(color);
                });
        }

        // ── Trend Table ────────────────────────────────────────────────
        private static void ComposeTrendTable(IContainer container, FinancialReportDto report)
        {
            if (!report.MonthlyTrend.Any()) return;

            container.Column(col =>
            {
                col.Item().PaddingBottom(6).Text("6-Month Financial Trend").Bold().FontSize(11);

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(2.5f);
                        cols.RelativeColumn(2f);
                        cols.RelativeColumn(2f);
                        cols.RelativeColumn(2f);
                    });

                    // Header row
                    static void HeaderCell(IContainer c, string text) =>
                        c.Background("#0d6efd").Padding(7)
                         .Text(text).Bold().FontSize(9).FontColor("#ffffff");

                    table.Header(h =>
                    {
                        h.Cell().Element(c => HeaderCell(c, "Month"));
                        h.Cell().Element(c => HeaderCell(c, "Expenses"));
                        h.Cell().Element(c => HeaderCell(c, "Income"));
                        h.Cell().Element(c => HeaderCell(c, "Net"));
                    });

                    // Data rows
                    bool even = false;
                    foreach (var t in report.MonthlyTrend)
                    {
                        var bg = even ? "#ffffff" : "#f8f9fa";
                        decimal net = t.Incomes - t.Expenses;
                        bool pos = net >= 0;

                        table.Cell().Background(bg).Padding(7).Text(t.Month).FontSize(9);
                        table.Cell().Background(bg).Padding(7).AlignRight()
                             .Text(t.Expenses.ToString("N2")).FontSize(9).FontColor("#dc3545");
                        table.Cell().Background(bg).Padding(7).AlignRight()
                             .Text(t.Incomes.ToString("N2")).FontSize(9).FontColor("#198754");
                        table.Cell().Background(bg).Padding(7).AlignRight()
                             .Text($"{(pos ? "+" : "")}{net:N2}").Bold().FontSize(9)
                             .FontColor(pos ? "#198754" : "#dc3545");

                        even = !even;
                    }
                });
            });
        }

        // ── Category Table ─────────────────────────────────────────────
        private static void ComposeCategoryTable(IContainer container, FinancialReportDto report)
        {
            if (!report.CategoryBreakdown.Any()) return;

            container.Column(col =>
            {
                col.Item().PaddingBottom(6).Text("Category Breakdown").Bold().FontSize(11);

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(3f);
                        cols.RelativeColumn(2f);
                        cols.RelativeColumn(1.5f);
                        cols.RelativeColumn(1.5f);
                    });

                    static void HeaderCell(IContainer c, string text) =>
                        c.Background("#0d6efd").Padding(7)
                         .Text(text).Bold().FontSize(9).FontColor("#ffffff");

                    table.Header(h =>
                    {
                        h.Cell().Element(c => HeaderCell(c, "Category"));
                        h.Cell().Element(c => HeaderCell(c, "Amount"));
                        h.Cell().Element(c => HeaderCell(c, "Transactions"));
                        h.Cell().Element(c => HeaderCell(c, "% of Total"));
                    });

                    bool even = false;
                    foreach (var cat in report.CategoryBreakdown)
                    {
                        var bg = even ? "#ffffff" : "#f8f9fa";

                        table.Cell().Background(bg).Padding(7).Text(cat.Category).FontSize(9);
                        table.Cell().Background(bg).Padding(7).AlignRight()
                             .Text(cat.Amount.ToString("N2")).Bold().FontSize(9);
                        table.Cell().Background(bg).Padding(7).AlignCenter()
                             .Text(cat.TransactionCount.ToString()).FontSize(9).FontColor("#6c757d");
                        table.Cell().Background(bg).Padding(7).AlignRight()
                             .Text($"{cat.Percentage:N1}%").FontSize(9).FontColor("#0d6efd");

                        even = !even;
                    }
                });
            });
        }

        // ── Footer ─────────────────────────────────────────────────────
        private static void ComposeFooter(IContainer container)
        {
            container
                .BorderTop(1).BorderColor("#dee2e6")
                .PaddingTop(6)
                .Row(row =>
                {
                    row.RelativeItem()
                        .Text("Generated by Spendly")
                        .FontSize(8).FontColor("#6c757d");

                    row.RelativeItem().AlignRight().Text(x =>
                    {
                        x.Span("Page ").FontSize(8).FontColor("#6c757d");
                        x.CurrentPageNumber().FontSize(8).FontColor("#6c757d");
                        x.Span(" of ").FontSize(8).FontColor("#6c757d");
                        x.TotalPages().FontSize(8).FontColor("#6c757d");
                    });
                });
        }

        // ── CSV ────────────────────────────────────────────────────────
        public byte[] GenerateCsv(FinancialReportDto report)
        {
            var sb = new StringBuilder();

            sb.AppendLine("SPENDLY - FINANCIAL REPORT");
            sb.AppendLine($"Period,{report.PeriodLabel}");
            sb.AppendLine($"Generated,{DateTime.Now:yyyy-MM-dd HH:mm}");
            sb.AppendLine();

            sb.AppendLine("SUMMARY");
            sb.AppendLine("Metric,Value");
            sb.AppendLine($"Total Expenses,{report.TotalExpenses:F2}");
            sb.AppendLine($"Total Income,{report.TotalIncomes:F2}");
            sb.AppendLine($"Net Balance,{report.NetBalance:F2}");
            sb.AppendLine($"Daily Average,{report.AverageDailyExpense:F2}");
            sb.AppendLine($"Top Category,{report.TopCategory}");
            sb.AppendLine($"Expense Ratio (%),{report.ExpenseToIncomeRatio:F2}");
            sb.AppendLine();

            if (report.MonthlyTrend.Any())
            {
                sb.AppendLine("6-MONTH TREND");
                sb.AppendLine("Month,Expenses,Income,Net");
                foreach (var t in report.MonthlyTrend)
                    sb.AppendLine($"{t.Month},{t.Expenses:F2},{t.Incomes:F2},{t.Incomes - t.Expenses:F2}");
                sb.AppendLine();
            }

            if (report.CategoryBreakdown.Any())
            {
                sb.AppendLine("CATEGORY BREAKDOWN");
                sb.AppendLine("Category,Amount,Transactions,% of Total");
                foreach (var cat in report.CategoryBreakdown)
                    sb.AppendLine($"{cat.Category},{cat.Amount:F2},{cat.TransactionCount},{cat.Percentage:F2}");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}
