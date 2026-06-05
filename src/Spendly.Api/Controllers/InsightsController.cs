using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spendly.Api.Extensions;
using Spendly.Application.Interfaces;
using Spendly.Application.UseCases.Insights;

namespace Spendly.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class InsightsController : ControllerBase
    {
        private readonly GetMonthlyInsightsUseCase _getInsights;
        private readonly IUserRepository _userRepo;
        private readonly IDateTimeProvider _dateTime;

        public InsightsController(GetMonthlyInsightsUseCase getInsights, IUserRepository userRepo, IDateTimeProvider dateTime)
        {
            _getInsights = getInsights;
            _userRepo = userRepo;
            _dateTime = dateTime;
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
            var user = await _userRepo.GetByIdAsync(userId);
            var now = _dateTime.Now(user?.TimeZone);
            var result = await _getInsights.ExecuteAsync(userId, year ?? now.Year, month ?? now.Month, user?.TimeZone);
            return Ok(result);
        }
    }
}
