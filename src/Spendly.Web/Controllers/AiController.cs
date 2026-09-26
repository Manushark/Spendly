using Microsoft.AspNetCore.Mvc;
using Spendly.Application.DTOs.Ai;
using Spendly.Web.Services;

namespace Spendly.Web.Controllers
{
    public class AiController : Controller
    {
        private readonly AiApiClient _api;
        private readonly ILogger<AiController> _logger;

        public AiController(AiApiClient api, ILogger<AiController> logger)
        {
            _api = api;
            _logger = logger;
        }

        private bool IsAuthenticated()
        {
            var token = HttpContext.Session.GetString("token");
            return !string.IsNullOrEmpty(token);
        }

        [HttpPost]
        public async Task<IActionResult> Parse([FromBody] AiCommandRequestDto dto, CancellationToken cancellationToken)
        {
            if (!IsAuthenticated())
            {
                return Unauthorized(new { success = false, message = "Session expired. Please log in again." });
            }

            if (string.IsNullOrWhiteSpace(dto?.Prompt))
            {
                return BadRequest(new { success = false, message = "Please provide a voice or text command." });
            }

            try
            {
                var plan = await _api.ParseCommandAsync(dto.Prompt, cancellationToken);
                if (plan == null)
                {
                    return StatusCode(500, new { success = false, message = "Could not interpret command. Please try again." });
                }

                return Json(new { success = true, plan });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing AI command");
                return StatusCode(500, new { success = false, message = "An error occurred while communicating with the AI service." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Execute([FromBody] AiFinancialPlanDto plan, CancellationToken cancellationToken)
        {
            if (!IsAuthenticated())
            {
                return Unauthorized(new { success = false, message = "Session expired. Please log in again." });
            }

            if (plan == null || !plan.HasActions)
            {
                return BadRequest(new { success = false, message = "No valid expenses or budgets to register." });
            }

            try
            {
                var result = await _api.ExecutePlanAsync(plan, cancellationToken);
                if (result == null || !result.Success)
                {
                    return StatusCode(500, new { success = false, message = result?.Message ?? "Failed to register expenses/budgets." });
                }

                return Json(new { success = true, result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing AI financial plan");
                return StatusCode(500, new { success = false, message = "An error occurred while saving the financial plan." });
            }
        }
    }
}
