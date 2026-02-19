using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spendly.Api.Extensions;
using Spendly.Application.DTOs.Expense;
using Spendly.Application.UseCase.CreateExpense;
using Spendly.Application.UseCase.DeleteExpense;
using Spendly.Application.UseCase.GetExpenseById;
using Spendly.Application.UseCase.ListExpenses;
using Spendly.Application.UseCases.Expenses;

namespace Spendly.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ExpensesController : ControllerBase
    {
        private readonly CreateExpenseUseCase _createExpenseUseCase;
        private readonly ListExpensesUseCase _listExpensesUseCase;
        private readonly GetExpenseByIdUseCase _getExpenseByIdUseCase;
        private readonly DeleteExpenseUseCase _deleteExpenseUseCase;
        private readonly UpdateExpenseUseCase _updateExpenseUseCase;

        public ExpensesController(
            CreateExpenseUseCase createExpenseUseCase,
            ListExpensesUseCase listExpensesUseCase,
            GetExpenseByIdUseCase getExpenseByIdUseCase,
            DeleteExpenseUseCase deleteExpenseUseCase,
            UpdateExpenseUseCase updateExpenseUseCase)
        {
            _createExpenseUseCase = createExpenseUseCase;
            _listExpensesUseCase = listExpensesUseCase;
            _getExpenseByIdUseCase = getExpenseByIdUseCase;
            _deleteExpenseUseCase = deleteExpenseUseCase;
            _updateExpenseUseCase = updateExpenseUseCase;
        }

        [HttpPost]
        public IActionResult Create([FromBody] CreateExpenseDto dto)
        {
            var userId = User.GetUserId();
            _createExpenseUseCase.Execute(userId, dto);
            return Ok();
        }

        [HttpGet]
        public IActionResult GetAll(
            [FromQuery] string? category,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page <= 0 || pageSize <= 0)
                return BadRequest("Page and pageSize must be greater than zero.");

            if (pageSize > 100)
                return BadRequest("pageSize cannot exceed 100.");

            var userId = User.GetUserId();
            var result = _listExpensesUseCase.Execute(userId, category, page, pageSize);
            return Ok(result);
        }



        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var userId = User.GetUserId();
            var result = _getExpenseByIdUseCase.Execute(userId, id);

            if (result == null) return NotFound();

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var userId = User.GetUserId();
            var deleted = _deleteExpenseUseCase.Execute(userId, id);

            if (!deleted) return NotFound();

            return NoContent();
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] UpdateExpenseDto dto)
        {
            var userId = User.GetUserId();
            _updateExpenseUseCase.Execute(userId, id, dto);
            return NoContent();
        }





    }
}
