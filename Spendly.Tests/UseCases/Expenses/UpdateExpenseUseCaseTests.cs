using Spendly.Application.DTOs.Expense;

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
    }
}
