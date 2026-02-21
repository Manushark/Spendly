//using Spendly.Application.UseCase.DeleteExpense;
//using Spendly.Application.UseCases.Expenses;
//using Spendly.Domain.Entities;
//using Spendly.Domain.ValueObjects;
//using Spendly.Tests.Fakes;
//using Xunit;

//namespace Spendly.Tests.UseCases.Expenses
//{
//    public class DeleteExpenseUseCaseTests
//    {
//        [Fact]
//        public void Execute_Should_Delete_Expense_When_Exists()
//        {
//            // Arrange
//            var repository = new FakeExpenseRepository();

//            var expense = Expense.Create(
//                Money.FromDecimal(100),
//                "Lunch",
//                DateTime.Now,
//                "Food"
//            );
//            repository.Add(expense);

//            var useCase = new DeleteExpenseUseCase(repository);

//            // Act
//            useCase.Execute(userId: 1, id: 5);

//            // Assert
//            Assert.Empty(repository.GetAll(null, 1, 10));
//        }
//    }
//}
