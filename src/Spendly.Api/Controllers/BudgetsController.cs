using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spendly.Api.Extensions;
using Spendly.Application.DTOs.Budget;
using Spendly.Application.UseCases.Budgets;

namespace Spendly.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BudgetsController : ControllerBase
    {
        private readonly CreateBudgetUseCase _create;
        private readonly UpdateBudgetUseCase _update;
        private readonly DeleteBudgetUseCase _delete;
        private readonly GetBudgetByIdUseCase _getById;
        private readonly GetBudgetSummaryUseCase _getSummary;

        public BudgetsController(
            CreateBudgetUseCase create,
            UpdateBudgetUseCase update,
            DeleteBudgetUseCase delete,
            GetBudgetByIdUseCase getById,
            GetBudgetSummaryUseCase getSummary)
        {
            _create = create;
            _update = update;
            _delete = delete;
            _getById = getById;
            _getSummary = getSummary;
        }

        /// <summary>
        /// POST /api/budgets
        /// Creates a new budget for the authenticated user
        /// </summary>
        [HttpPost]
        public IActionResult Create([FromBody] CreateBudgetDto dto)
        {
            _create.Execute(User.GetUserId(), dto);
            return Ok(new { message = "Budget created successfully" });
        }

        /// <summary>
        /// GET /api/budgets/summary?year=2026&month=2
        /// Returns all budgets for a specific month with spending data
        /// </summary>
        [HttpGet("summary")]
        public IActionResult GetSummary([FromQuery] int? year, [FromQuery] int? month)
        {
            var now = DateTime.Now;
            var targetYear = year ?? now.Year;
            var targetMonth = month ?? now.Month;

            var summary = _getSummary.Execute(User.GetUserId(), targetYear, targetMonth);
            return Ok(summary);
        }

        /// <summary>
        /// GET /api/budgets/{id}
        /// Returns a single budget with spending data
        /// </summary>
        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var budget = _getById.Execute(User.GetUserId(), id);
            return budget == null ? NotFound() : Ok(budget);
        }

        /// <summary>
        /// PUT /api/budgets/{id}
        /// Updates an existing budget
        /// </summary>
        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] UpdateBudgetDto dto)
        {
            _update.Execute(User.GetUserId(), id, dto);
            return NoContent();
        }

        /// <summary>
        /// DELETE /api/budgets/{id}
        /// Deletes a budget
        /// </summary>
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var deleted = _delete.Execute(User.GetUserId(), id);
            return deleted ? NoContent() : NotFound();
        }
    }
}