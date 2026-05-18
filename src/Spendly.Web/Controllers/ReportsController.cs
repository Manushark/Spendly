using Microsoft.AspNetCore.Mvc;
using Spendly.Web.Services;

namespace Spendly.Web.Controllers
{
    /// <summary>
    /// Controlador MVC que muestra y exporta el reporte financiero avanzado.
    /// Soporta presets de fecha (este mes, últimos 90 días, año a la fecha, etc.)
    /// y rango personalizado via Flatpickr.
    /// </summary>
    public class ReportsController : Controller
    {
        private readonly ReportApiClient   _api;
        private readonly ReportExportService _export;

        public ReportsController(ReportApiClient api, ReportExportService export)
        {
            _api    = api;
            _export = export;
        }

        // ── Index ──────────────────────────────────────────────────────────────
        public async Task<IActionResult> Index(
            string? preset   = null,
            string? dateFrom = null,
            string? dateTo   = null)
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var (from, to, label) = ResolvePreset(preset, dateFrom, dateTo);

            var report = await _api.GetReportAsync(from, to, label);

            if (report == null)
            {
                TempData["Error"] = "Could not load the financial report. Please try again.";
                return RedirectToAction("Index", "Dashboard");
            }

            SetViewBag(from, to, preset ?? "this_month", label);
            return View(report);
        }

        // ── Export PDF ─────────────────────────────────────────────────────────
        public async Task<IActionResult> ExportPdf(string? dateFrom, string? dateTo, string? preset)
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var (from, to, label) = ResolvePreset(preset, dateFrom, dateTo);

            var report = await _api.GetReportAsync(from, to, label);
            if (report == null)
                return BadRequest("Could not retrieve report data.");

            var pdfBytes = _export.GeneratePdf(report);
            var safeName = report.PeriodLabel.Replace(" ", "-").Replace("/", "-").Replace("–", "to");
            var fileName = $"spendly-report-{safeName}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }

        // ── Export CSV ─────────────────────────────────────────────────────────
        public async Task<IActionResult> ExportCsv(string? dateFrom, string? dateTo, string? preset)
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var (from, to, label) = ResolvePreset(preset, dateFrom, dateTo);

            var report = await _api.GetReportAsync(from, to, label);
            if (report == null)
                return BadRequest("Could not retrieve report data.");

            var csvBytes = _export.GenerateCsv(report);
            var safeName = report.PeriodLabel.Replace(" ", "-").Replace("/", "-").Replace("–", "to");
            var fileName = $"spendly-report-{safeName}.csv";

            return File(csvBytes, "text/csv", fileName);
        }

        // ── CategoryTransactions (AJAX drill-down) ─────────────────────────────
        [HttpGet]
        public async Task<IActionResult> CategoryTransactions(
            string? dateFrom, string? dateTo, string? preset, string category)
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token))
                return Unauthorized();

            var (from, to, _) = ResolvePreset(preset, dateFrom, dateTo);
            var transactions   = await _api.GetCategoryTransactionsAsync(from, to, category);
            return Json(transactions);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// Resuelve el rango de fechas y la etiqueta según el preset seleccionado o fechas personalizadas.
        /// </summary>
        private static (DateTime from, DateTime to, string label) ResolvePreset(
            string? preset, string? dateFrom, string? dateTo)
        {
            var now   = DateTime.Now;
            var today = now.Date;

            // Si hay fechas personalizadas y el preset es "custom"
            if (preset == "custom"
                && DateTime.TryParse(dateFrom, out var customFrom)
                && DateTime.TryParse(dateTo,   out var customTo))
            {
                return (customFrom.Date, customTo.Date,
                    $"{customFrom:d MMM yyyy} – {customTo:d MMM yyyy}");
            }

            return preset switch
            {
                "last_month" => (
                    new DateTime(today.Year, today.Month, 1).AddMonths(-1),
                    new DateTime(today.Year, today.Month, 1).AddDays(-1),
                    new DateTime(today.Year, today.Month, 1).AddMonths(-1).ToString("MMMM yyyy")),

                "last_3_months" => (
                    today.AddDays(-89),
                    today,
                    "Last 90 Days"),

                "last_6_months" => (
                    today.AddDays(-179),
                    today,
                    "Last 6 Months"),

                "ytd" => (
                    new DateTime(today.Year, 1, 1),
                    today,
                    $"Year to Date {today.Year}"),

                "last_year" => (
                    new DateTime(today.Year - 1, 1, 1),
                    new DateTime(today.Year - 1, 12, 31),
                    $"Year {today.Year - 1}"),

                // Default / "this_month"
                _ => (
                    new DateTime(today.Year, today.Month, 1),
                    today,
                    today.ToString("MMMM yyyy"))
            };
        }

        private void SetViewBag(DateTime from, DateTime to, string activePreset, string periodLabel)
        {
            ViewBag.DateFrom      = from.ToString("yyyy-MM-dd");
            ViewBag.DateTo        = to.ToString("yyyy-MM-dd");
            ViewBag.ActivePreset  = activePreset;
            ViewBag.PeriodLabel   = periodLabel;
        }
    }
}
