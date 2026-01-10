using Microsoft.AspNetCore.Mvc;
using Spendly.Application.DTOs.Expense;
using Spendly.Application.UseCase.CreateExpense;
using Spendly.Application.UseCase.GetExpenseById;
using Spendly.Application.UseCase.ListExpenses;

namespace Spendly.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExpensesController : ControllerBase
    {
        private readonly CreateExpenseUseCase _createExpenseUseCase;
        private readonly ListExpensesUseCase _listExpensesUseCase;
        private readonly GetExpenseByIdUseCase _getExpenseByIdUseCase;

        public ExpensesController(CreateExpenseUseCase createExpenseUseCase, ListExpensesUseCase listExpensesUseCase,GetExpenseByIdUseCase getExpenseByIdUseCase )
        {
            _createExpenseUseCase = createExpenseUseCase;
            _listExpensesUseCase = listExpensesUseCase;
            _getExpenseByIdUseCase = getExpenseByIdUseCase;
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


    }
}
