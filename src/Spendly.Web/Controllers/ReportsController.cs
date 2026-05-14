using Microsoft.AspNetCore.Mvc;
using Spendly.Web.Services;

namespace Spendly.Web.Controllers
{
    /// <summary>
    /// Controlador MVC que muestra el reporte financiero avanzado al usuario.
    /// </summary>
    public class ReportsController : Controller
    {
        private readonly ReportApiClient _api;

        public ReportsController(ReportApiClient api)
        {
            _api = api;
        }

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

            // Pass navigation helpers to the view
            ViewBag.CurrentYear  = targetYear;
            ViewBag.CurrentMonth = targetMonth;
            ViewBag.PrevYear     = targetMonth == 1 ? targetYear - 1 : targetYear;
            ViewBag.PrevMonth    = targetMonth == 1 ? 12 : targetMonth - 1;
            ViewBag.NextYear     = targetMonth == 12 ? targetYear + 1 : targetYear;
            ViewBag.NextMonth    = targetMonth == 12 ? 1 : targetMonth + 1;
            ViewBag.IsCurrentMonth = (targetYear == now.Year && targetMonth == now.Month);

            return View(report);
        }
    }
}
