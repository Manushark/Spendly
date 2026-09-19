using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Spendly.Api.Extensions;
using Spendly.Api.Security;
using Spendly.Application.DTOs.Ai;
using Spendly.Application.UseCases.Ai;

namespace Spendly.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/ai")]
    public class AiController : ControllerBase
    {
        private readonly ParseAiCommandUseCase _parseAiCommandUseCase;
        private readonly ExecuteAiPlanUseCase _executeAiPlanUseCase;

        public AiController(
            ParseAiCommandUseCase parseAiCommandUseCase,
            ExecuteAiPlanUseCase executeAiPlanUseCase)
        {
            _parseAiCommandUseCase = parseAiCommandUseCase;
            _executeAiPlanUseCase = executeAiPlanUseCase;
        }

        [HttpPost("parse")]
        [EnableRateLimiting(RateLimitPolicies.WriteOperations)]
        public async Task<IActionResult> ParseCommand([FromBody] AiCommandRequestDto dto, CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            var plan = await _parseAiCommandUseCase.ExecuteAsync(userId, dto.Prompt, cancellationToken);
            return Ok(plan);
        }

        [HttpPost("execute")]
        [EnableRateLimiting(RateLimitPolicies.WriteOperations)]
        public async Task<IActionResult> ExecutePlan([FromBody] AiFinancialPlanDto plan)
        {
            var userId = User.GetUserId();
            var result = await _executeAiPlanUseCase.ExecuteAsync(userId, plan);
            return Ok(result);
        }
    }
}
