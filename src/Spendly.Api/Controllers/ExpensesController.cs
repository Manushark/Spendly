using Microsoft.AspNetCore.Mvc;
using Spendly.Application.DTOs.Expense;
using Spendly.Application.UseCase.CreateExpense;

namespace Spendly.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExpensesController : ControllerBase
    {
        private readonly CreateExpenseUseCase _createExpenseUseCase;

        public ExpensesController(CreateExpenseUseCase createExpenseUseCase)
        {
            _createExpenseUseCase = createExpenseUseCase;
        }

        [HttpPost]
        public IActionResult Create([FromBody] CreateExpenseDto dto)
        {
            _createExpenseUseCase.Execute(dto);
            return Ok();
        }
    }
}
