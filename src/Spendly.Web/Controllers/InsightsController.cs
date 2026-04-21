using Microsoft.AspNetCore.Mvc;
using Spendly.Web.Contracts.Insights;
using Spendly.Web.Services;

namespace Spendly.Web.Controllers
{
    public class InsightsController : Controller
    {
        private readonly InsightsApiClient _api;

        public InsightsController(InsightsApiClient api) => _api = api;

        public async Task<IActionResult> Index(int? month = null, int? year = null)
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var data = await _api.GetMonthlyInsightsAsync(month, year);
            data ??= new MonthlyInsightsDto
            {
                Year = year ?? DateTime.UtcNow.Year,
                Month = month ?? DateTime.UtcNow.Month,
                MonthName = new DateTime(year ?? DateTime.UtcNow.Year, month ?? DateTime.UtcNow.Month, 1).ToString("MMMM yyyy")
            };

            return View(data);
        }
    }
}
