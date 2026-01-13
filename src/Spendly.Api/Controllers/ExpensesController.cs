using Microsoft.AspNetCore.Mvc;
using Spendly.Application.DTOs.Expense;
using Spendly.Application.UseCase.CreateExpense;
using Spendly.Application.UseCase.DeleteExpense;
using Spendly.Application.UseCase.GetExpenseById;
using Spendly.Application.UseCase.ListExpenses;
using Spendly.Application.UseCases.Expenses;

namespace Spendly.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExpensesController : ControllerBase
    {
        private readonly CreateExpenseUseCase _createExpenseUseCase;
        private readonly ListExpensesUseCase _listExpensesUseCase;
        private readonly GetExpenseByIdUseCase _getExpenseByIdUseCase;
        private readonly DeleteExpenseUseCase _deleteExpenseUseCase;
        private readonly UpdateExpenseUseCase _updateExpenseUseCase;

        public ExpensesController(CreateExpenseUseCase createExpenseUseCase, ListExpensesUseCase listExpensesUseCase,
            GetExpenseByIdUseCase getExpenseByIdUseCase,
            DeleteExpenseUseCase deleteExpenseUseCase, UpdateExpenseUseCase updateExpenseUseCase)
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
            _createExpenseUseCase.Execute(dto);
            return Ok();
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var result = _listExpensesUseCase.Execute();
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var result = _getExpenseByIdUseCase.Execute(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var deleted = _deleteExpenseUseCase.Execute(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, UpdateExpenseDto dto)
        {
            _updateExpenseUseCase.Execute(id, dto);
            return NoContent();
        }





    }
}
