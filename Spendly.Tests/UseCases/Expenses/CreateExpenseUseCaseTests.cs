using Spendly.Application.DTOs.Expense;
using Spendly.Application.UseCase.CreateExpense;
using Spendly.Tests.Fakes;
using Xunit;

namespace Spendly.Tests.UseCases.Expenses
{
    public class CreateExpenseUseCaseTests
    {
        [Fact]
        public void Execute_Should_Create_Expense_When_Data_Is_Valid()
        {
            // Arrange
            var repository = new FakeExpenseRepository();
            var useCase = new CreateExpenseUseCase(repository);

            var dto = new CreateExpenseDto
            {
                Amount = 100,
                Description = "Lunch",
                Date = DateTime.Now,
                Category = "Food"
            };

            // Act
            useCase.Execute(dto);

            // Assert
            var expenses = repository.GetAll(
                category: null,
                page: 1,
                pageSize: 10
            );

            Assert.Single(expenses);
        }
    }
}
