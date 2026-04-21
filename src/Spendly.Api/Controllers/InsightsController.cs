using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spendly.Api.Extensions;
using Spendly.Application.UseCases.Insights;

namespace Spendly.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class InsightsController : ControllerBase
    {
        private readonly GetMonthlyInsightsUseCase _getInsights;

        public InsightsController(GetMonthlyInsightsUseCase getInsights)
        {
            _getInsights = getInsights;
        }

        /// <summary>
        /// GET /api/insights?month=4&year=2026
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMonthlyInsights(
            [FromQuery] int? month = null,
            [FromQuery] int? year = null)
        {
            var userId = User.GetUserId();
            var now = DateTime.UtcNow;
            var result = await _getInsights.ExecuteAsync(userId, year ?? now.Year, month ?? now.Month);
            return Ok(result);
        }
    }
}
