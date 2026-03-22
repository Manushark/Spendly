using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spendly.Api.Extensions;
using Spendly.Application.Services;
using Spendly.Domain.Entities;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class InsightsController : ControllerBase
{
    private readonly AIInsightsService _aiService;

    public InsightsController(AIInsightsService aiService)
    {
        _aiService = aiService;
    }

    [HttpGet]
    public async Task<IActionResult> GetInsights()
    {
        var userId = User.GetUserId();
        var insights = await _aiService.GenerateInsightsAsync(userId);
        return Ok(insights);
    }
}