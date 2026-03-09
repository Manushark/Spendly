using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spendly.Api.Extensions;
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
        public IActionResult GetAll()
        {
            var summary = _getSummary.Execute(User.GetUserId());
            return Ok(summary);
        }

        /// <summary>
        /// GET /api/recurring-expenses/{id}
        /// Returns a single recurring expense
        /// </summary>
        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var recurring = _getById.Execute(User.GetUserId(), id);
            return recurring == null ? NotFound() : Ok(recurring);
        }

        /// <summary>
        /// POST /api/recurring-expenses
        /// Creates a new recurring expense template
        /// </summary>
        [HttpPost]
        public IActionResult Create([FromBody] CreateRecurringExpenseDto dto)
        {
            _create.Execute(User.GetUserId(), dto);
            return Ok(new { message = "Recurring expense created successfully" });
        }

        /// <summary>
        /// PUT /api/recurring-expenses/{id}
        /// Updates an existing recurring expense
        /// </summary>
        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] UpdateRecurringExpenseDto dto)
        {
            _update.Execute(User.GetUserId(), id, dto);
            return NoContent();
        }

        /// <summary>
        /// POST /api/recurring-expenses/{id}/toggle
        /// Activates or pauses a recurring expense
        /// </summary>
        [HttpPost("{id:int}/toggle")]
        public IActionResult Toggle(int id, [FromBody] ToggleRequest request)
        {
            _toggle.Execute(User.GetUserId(), id, request.Activate);
            var action = request.Activate ? "activated" : "paused";
            return Ok(new { message = $"Recurring expense {action} successfully" });
        }

        /// <summary>
        /// DELETE /api/recurring-expenses/{id}
        /// Deletes a recurring expense
        /// </summary>
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var deleted = _delete.Execute(User.GetUserId(), id);
            return deleted ? NoContent() : NotFound();
        }
    }

    public record ToggleRequest(bool Activate);
}