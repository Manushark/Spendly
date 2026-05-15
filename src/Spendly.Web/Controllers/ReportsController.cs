using Microsoft.AspNetCore.Mvc;
using Spendly.Web.Services;

namespace Spendly.Web.Controllers
{
    /// <summary>
    /// Controlador MVC que muestra y exporta el reporte financiero avanzado.
    /// </summary>
    public class ReportsController : Controller
    {
        private readonly ReportApiClient _api;
        private readonly ReportExportService _export;

        public ReportsController(ReportApiClient api, ReportExportService export)
        {
            _api    = api;
            _export = export;
        }

        // ── Index ──────────────────────────────────────────────────────
        public async Task<IActionResult> Index(int? year, int? month)
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var now = DateTime.Now;
            var targetYear  = year  ?? now.Year;
            var targetMonth = month ?? now.Month;

            var report = await _api.GetReportAsync(targetYear, targetMonth);

            if (report == null)
            {
                TempData["Error"] = "Could not load the financial report. Please try again.";
                return RedirectToAction("Index", "Dashboard");
            }

            SetNavigationViewBag(targetYear, targetMonth, now);
            return View(report);
        }

        // ── Export PDF ────────────────────────────────────────────────
        public async Task<IActionResult> ExportPdf(int? year, int? month)
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var now = DateTime.Now;
            var targetYear  = year  ?? now.Year;
            var targetMonth = month ?? now.Month;

            var report = await _api.GetReportAsync(targetYear, targetMonth);
            if (report == null)
                return BadRequest("Could not retrieve report data.");

            var pdfBytes = _export.GeneratePdf(report);
            var fileName = $"spendly-report-{report.PeriodLabel.Replace(" ", "-")}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }

        // ── Export CSV ────────────────────────────────────────────────
        public async Task<IActionResult> ExportCsv(int? year, int? month)
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var now = DateTime.Now;
            var targetYear  = year  ?? now.Year;
            var targetMonth = month ?? now.Month;

            var report = await _api.GetReportAsync(targetYear, targetMonth);
            if (report == null)
                return BadRequest("Could not retrieve report data.");

            var csvBytes = _export.GenerateCsv(report);
            var fileName = $"spendly-report-{report.PeriodLabel.Replace(" ", "-")}.csv";

            return File(csvBytes, "text/csv", fileName);
        }

        // ── Helpers ───────────────────────────────────────────────────
        private void SetNavigationViewBag(int year, int month, DateTime now)
        {
            ViewBag.CurrentYear    = year;
            ViewBag.CurrentMonth   = month;
            ViewBag.PrevYear       = month == 1 ? year - 1 : year;
            ViewBag.PrevMonth      = month == 1 ? 12 : month - 1;
            ViewBag.NextYear       = month == 12 ? year + 1 : year;
            ViewBag.NextMonth      = month == 12 ? 1 : month + 1;
            ViewBag.IsCurrentMonth = (year == now.Year && month == now.Month);
        }
    }
}
