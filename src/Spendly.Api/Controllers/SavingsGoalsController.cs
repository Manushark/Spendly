using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spendly.Api.Extensions;
using Spendly.Application.DTOs.SavingsGoal;
using Spendly.Application.UseCases.SavingsGoals;

namespace Spendly.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/savings-goals")]
    public class SavingsGoalsController : ControllerBase
    {
        private readonly CreateSavingsGoalUseCase _create;
        private readonly UpdateSavingsGoalUseCase _update;
        private readonly DeleteSavingsGoalUseCase _delete;
        private readonly AddFundsUseCase _addFunds;
        private readonly ListSavingsGoalsUseCase _list;
        private readonly GetSavingsGoalByIdUseCase _getById;

        public SavingsGoalsController(
            CreateSavingsGoalUseCase create,
            UpdateSavingsGoalUseCase update,
            DeleteSavingsGoalUseCase delete,
            AddFundsUseCase addFunds,
            ListSavingsGoalsUseCase list,
            GetSavingsGoalByIdUseCase getById)
        {
            _create = create;
            _update = update;
            _delete = delete;
            _addFunds = addFunds;
            _list = list;
            _getById = getById;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var goals = await _list.ExecuteAsync(User.GetUserId());
            return Ok(goals);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var goal = await _getById.ExecuteAsync(User.GetUserId(), id);
            if (goal == null) return NotFound();
            return Ok(goal);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSavingsGoalDto dto)
        {
            var id = await _create.ExecuteAsync(User.GetUserId(), dto);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSavingsGoalDto dto)
        {
            await _update.ExecuteAsync(User.GetUserId(), id, dto);
            return NoContent();
        }

        [HttpPost("{id:int}/add-funds")]
        public async Task<IActionResult> AddFunds(int id, [FromBody] AddFundsDto dto)
        {
            await _addFunds.ExecuteAsync(User.GetUserId(), id, dto.Amount);
            return Ok(new { message = "Funds added successfully" });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _delete.ExecuteAsync(User.GetUserId(), id);
            return NoContent();
        }
    }
}
