using Spendly.Application.DTOs.Expense;
using Spendly.Application.UseCases.Expenses;
using Spendly.Domain.Entities;
using Spendly.Domain.Exceptions;
using Spendly.Tests.Fakes;
using Xunit;

namespace Spendly.Tests.UseCases.Expenses
{
    public class UpdateExpenseUseCaseTests
    {
        [Fact]
        public void Execute_Should_Update_Expense_When_Expense_Exists()
        {
            // -------- Arrange --------
            var expense = new Expense(
                100,
                "Lunch",
                DateTime.Now,
                "Food"
            );

            var repository = new FakeExpenseRepository();
            repository.Add(expense);

            var useCase = new UpdateExpenseUseCase(repository);

            var dto = new UpdateExpenseDto
            {
                Amount = 150,
                Description = "Dinner",
                Date = DateTime.Now,
                Category = "Food"
            };

            // -------- Act --------
            useCase.Execute(expense.Id, dto);

            // -------- Assert --------
            Assert.Equal(150, expense.Amount);
            Assert.Equal("Dinner", expense.Description);

        }

        [Fact]
        public void Execute_Should_Throw_Exception_When_Expense_Does_Not_Exist()
        {
            var repository = new FakeExpenseRepository();
            var useCase = new UpdateExpenseUseCase(repository);

            var dto = new UpdateExpenseDto
            {
                Amount = 100,
                Description = "Test",
                Date = DateTime.Now,
                Category = "Test"
            };

            Assert.Throws<ExpenseNotFoundException>(() =>
                useCase.Execute(999, dto)
            );
        }

    }
}
