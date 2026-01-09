using Microsoft.AspNetCore.Mvc;
using Spendly.Application.Interfaces;
using Spendly.Domain.Entities;

namespace Spendly.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExpensesController : Controller
    {
        private readonly IExpenseRepository _expenseRepository;

        public ExpensesController(IExpenseRepository expenseRepository)
        {
            _expenseRepository = expenseRepository;
        }

        [HttpPost]
        public IActionResult Create([FromBody] Expense expense)
        {
            _expenseRepository.Add(expense);
            return Ok(expense);
        }
    }
}
