using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Spendly.Api.Extensions;
using Spendly.Api.Security;
using Spendly.Application.DTOs.RecurringExpense;
using Spendly.Application.UseCases.RecurringExpenses;

namespace Spendly.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/recurring-expenses")]
    public class RecurringExpensesController : ControllerBase
    {
        private readonly CreateRecurringExpenseUseCase _create;
        private readonly UpdateRecurringExpenseUseCase _update;
        private readonly DeleteRecurringExpenseUseCase _delete;
        private readonly ToggleRecurringExpenseUseCase _toggle;
        private readonly GetRecurringExpenseSummaryUseCase _getSummary;
        private readonly GetRecurringExpenseByIdUseCase _getById;

        public RecurringExpensesController(
            CreateRecurringExpenseUseCase create,
            UpdateRecurringExpenseUseCase update,
            DeleteRecurringExpenseUseCase delete,
            ToggleRecurringExpenseUseCase toggle,
            GetRecurringExpenseSummaryUseCase getSummary,
            GetRecurringExpenseByIdUseCase getById)
        {
            _create = create;
            _update = update;
            _delete = delete;
            _toggle = toggle;
            _getSummary = getSummary;
            _getById = getById;
        }

        /// <summary>
        /// GET /api/recurring-expenses
        /// Returns all recurring expenses for the authenticated user
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var summary = await _getSummary.ExecuteAsync(User.GetUserId());
            return Ok(summary);
        }

        /// <summary>
        /// GET /api/recurring-expenses/{id}
        /// Returns a single recurring expense
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var recurring = await _getById.ExecuteAsync(User.GetUserId(), id);
            return recurring == null ? NotFound() : Ok(recurring);
        }

        /// <summary>
        /// POST /api/recurring-expenses
        /// Creates a new recurring expense template
        /// </summary>
        [EnableRateLimiting(RateLimitPolicies.WriteOperations)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRecurringExpenseDto dto)
        {
            await _create.ExecuteAsync(User.GetUserId(), dto);
            return Ok(new { message = "Recurring expense created successfully" });
        }

        /// <summary>
        /// PUT /api/recurring-expenses/{id}
        /// Updates an existing recurring expense
        /// </summary>
        [EnableRateLimiting(RateLimitPolicies.WriteOperations)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRecurringExpenseDto dto)
        {
            await _update.ExecuteAsync(User.GetUserId(), id, dto);
            return NoContent();
        }

        /// <summary>
        /// POST /api/recurring-expenses/{id}/toggle
        /// Activates or pauses a recurring expense
        /// </summary>
        [EnableRateLimiting(RateLimitPolicies.WriteOperations)]
        [HttpPost("{id:int}/toggle")]
        public async Task<IActionResult> Toggle(int id, [FromBody] ToggleRequest request)
        {
            await _toggle.ExecuteAsync(User.GetUserId(), id, request.Activate);
            var action = request.Activate ? "activated" : "paused";
            return Ok(new { message = $"Recurring expense {action} successfully" });
        }

        /// <summary>
        /// DELETE /api/recurring-expenses/{id}
        /// Deletes a recurring expense
        /// </summary>
        [EnableRateLimiting(RateLimitPolicies.WriteOperations)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _delete.ExecuteAsync(User.GetUserId(), id);
            return deleted ? NoContent() : NotFound();
        }
    }

    public record ToggleRequest(bool Activate);
}
