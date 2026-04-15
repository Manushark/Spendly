using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spendly.Api.Extensions;
using Spendly.Application.DTOs.Income;
using Spendly.Application.UseCases.Incomes;

namespace Spendly.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class IncomesController : ControllerBase
    {
        private readonly CreateIncomeUseCase _create;
        private readonly UpdateIncomeUseCase _update;
        private readonly DeleteIncomeUseCase _delete;
        private readonly ListIncomesUseCase _list;
        private readonly GetIncomeByIdUseCase _getById;

        public IncomesController(
            CreateIncomeUseCase create,
            UpdateIncomeUseCase update,
            DeleteIncomeUseCase delete,
            ListIncomesUseCase list,
            GetIncomeByIdUseCase getById)
        {
            _create = create;
            _update = update;
            _delete = delete;
            _list = list;
            _getById = getById;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page <= 0 || pageSize <= 0)
                return BadRequest("Page and pageSize must be greater than zero.");

            if (pageSize > 100)
                return BadRequest("pageSize cannot exceed 100.");

            var result = await _list.ExecuteAsync(User.GetUserId(), page, pageSize);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _getById.ExecuteAsync(User.GetUserId(), id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateIncomeDto dto)
        {
            var id = await _create.ExecuteAsync(User.GetUserId(), dto);
            return CreatedAtAction(nameof(GetById), new { id }, new { id, message = "Income created successfully" });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateIncomeDto dto)
        {
            await _update.ExecuteAsync(User.GetUserId(), id, dto);
            return Ok(new { message = "Income updated successfully" });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _delete.ExecuteAsync(User.GetUserId(), id);
            return deleted ? NoContent() : NotFound();
        }
    }
}
