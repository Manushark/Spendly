using Microsoft.AspNetCore.Mvc;

namespace Spendly.Web.Controllers
{
    public class InsightsController : Controller
    {
        private readonly InsightsApiClient _insightsClient;

        public InsightsController(InsightsApiClient insightsClient)
        {
            _insightsClient = insightsClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var insights = await _insightsClient.GetInsightsAsync();
                return View(insights);
            }
            catch (Exception)
            {
                ViewBag.Error = "Could not load insights. Please try again later.";
                return View(null);
            }
        }
    }
}