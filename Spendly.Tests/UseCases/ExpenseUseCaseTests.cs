using Moq;
using Spendly.Application.DTOs.Expense;
using Spendly.Application.Interfaces;
using Spendly.Application.UseCase.CreateExpense;
using Spendly.Application.UseCase.DeleteExpense;
using Spendly.Application.UseCase.GetExpenseById;
using Spendly.Application.UseCase.ListExpenses;
using Spendly.Application.UseCases.Expenses;
using Spendly.Domain.Entities;
using Spendly.Domain.Exceptions;
using Spendly.Domain.ValueObjects;
using Xunit;

namespace Spendly.Tests.UseCases
{
    public class ExpenseUseCaseTests
    {
        // ──────────────────────────────────────────
        // Helpers
        // ──────────────────────────────────────────
        private static Expense MakeExpense(int userId = 1, int id = 0)
        {
            var expense = Expense.Create(
                userId,
                Money.FromDecimal(50m),
                "Test expense",
                DateTime.UtcNow.AddDays(-1),
                "Food");

            // Simular Id asignado por EF (para pruebas)
            if (id > 0)
            {
                var idProp = typeof(Expense).GetProperty("Id");
                idProp?.SetValue(expense, id);
            }

            return expense;
        }

        // ──────────────────────────────────────────
        // CreateExpenseUseCase
        // ──────────────────────────────────────────

        [Fact]
        public void Create_ValidDto_CallsRepositoryAdd()
        {
            var repo = new Mock<IExpenseRepository>();
            var useCase = new CreateExpenseUseCase(repo.Object);

            var dto = new CreateExpenseDto
            {
                Amount = 100m,
                Description = "Groceries",
                Date = DateTime.UtcNow.AddDays(-1),
                Category = "Food"
            };

            useCase.Execute(userId: 1, dto);

            repo.Verify(r => r.Add(It.Is<Expense>(e =>
                e.UserId == 1 &&
                e.Amount.Value == 100m &&
                e.Description == "Groceries"
            )), Times.Once);
        }

        [Fact]
        public void Create_FutureDate_ThrowsDomainException()
        {
            var repo = new Mock<IExpenseRepository>();
            var useCase = new CreateExpenseUseCase(repo.Object);

            var dto = new CreateExpenseDto
            {
                Amount = 100m,
                Description = "Future",
                Date = DateTime.UtcNow.AddDays(5), // ← fecha futura
                Category = "Other"
            };

            Assert.Throws<InvalidDomainException>(() => useCase.Execute(1, dto));
            repo.Verify(r => r.Add(It.IsAny<Expense>()), Times.Never);
        }

        [Fact]
        public void Create_NegativeAmount_ThrowsArgumentException()
        {
            var repo = new Mock<IExpenseRepository>();
            var useCase = new CreateExpenseUseCase(repo.Object);

            var dto = new CreateExpenseDto
            {
                Amount = -10m,
                Description = "Bad",
                Date = DateTime.UtcNow.AddDays(-1),
                Category = "Food"
            };

            Assert.Throws<ArgumentException>(() => useCase.Execute(1, dto));
        }

        // ──────────────────────────────────────────
        // UpdateExpenseUseCase
        // ──────────────────────────────────────────

        [Fact]
        public void Update_OwnExpense_CallsRepositoryUpdate()
        {
            var expense = MakeExpense(userId: 1, id: 10);
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetById(10)).Returns(expense);

            var useCase = new UpdateExpenseUseCase(repo.Object);

            var dto = new UpdateExpenseDto
            {
                Amount = 200m,
                Description = "Updated",
                Date = DateTime.UtcNow.AddDays(-2),
                Category = "Transport"
            };

            useCase.Execute(userId: 1, id: 10, dto);

            repo.Verify(r => r.Update(expense), Times.Once);
        }

        [Fact]
        public void Update_OtherUserExpense_ThrowsUnauthorized()
        {
            var expense = MakeExpense(userId: 1, id: 10);  // pertenece a user 1
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetById(10)).Returns(expense);

            var useCase = new UpdateExpenseUseCase(repo.Object);

            var dto = new UpdateExpenseDto
            {
                Amount = 200m,
                Description = "Hack",
                Date = DateTime.UtcNow.AddDays(-1),
                Category = "Other"
            };

            // user 2 intenta modificar el gasto de user 1
            Assert.Throws<UnauthorizedExpenseAccessException>(() =>
                useCase.Execute(userId: 2, id: 10, dto));

            repo.Verify(r => r.Update(It.IsAny<Expense>()), Times.Never);
        }

        [Fact]
        public void Update_NotFound_ThrowsExpenseNotFoundException()
        {
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetById(99)).Returns((Expense?)null);

            var useCase = new UpdateExpenseUseCase(repo.Object);

            Assert.Throws<ExpenseNotFoundException>(() =>
                useCase.Execute(1, 99, new UpdateExpenseDto
                {
                    Amount = 10m,
                    Description = "x",
                    Date = DateTime.UtcNow.AddDays(-1),
                    Category = "x"
                }));
        }

        // ──────────────────────────────────────────
        // DeleteExpenseUseCase
        // ──────────────────────────────────────────

        [Fact]
        public void Delete_OwnExpense_ReturnsTrue()
        {
            var expense = MakeExpense(userId: 1, id: 5);
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetById(5)).Returns(expense);
            repo.Setup(r => r.Delete(5)).Returns(true);

            var useCase = new DeleteExpenseUseCase(repo.Object);
            var result = useCase.Execute(userId: 1, id: 5);

            Assert.True(result);
        }

        [Fact]
        public void Delete_OtherUserExpense_ThrowsUnauthorized()
        {
            var expense = MakeExpense(userId: 1, id: 5);
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetById(5)).Returns(expense);

            var useCase = new DeleteExpenseUseCase(repo.Object);

            Assert.Throws<UnauthorizedExpenseAccessException>(() =>
                useCase.Execute(userId: 2, id: 5));

            repo.Verify(r => r.Delete(It.IsAny<int>()), Times.Never);
        }

        // ──────────────────────────────────────────
        // GetExpenseByIdUseCase
        // ──────────────────────────────────────────

        [Fact]
        public void GetById_OwnExpense_ReturnsDto()
        {
            var expense = MakeExpense(userId: 1, id: 3);
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetById(3)).Returns(expense);

            var useCase = new GetExpenseByIdUseCase(repo.Object);
            var result = useCase.Execute(userId: 1, id: 3);

            Assert.NotNull(result);
            Assert.Equal("Test expense", result!.Description);
        }

        [Fact]
        public void GetById_NotFound_ReturnsNull()
        {
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetById(99)).Returns((Expense?)null);

            var useCase = new GetExpenseByIdUseCase(repo.Object);
            var result = useCase.Execute(userId: 1, id: 99);

            Assert.Null(result);
        }

        // ──────────────────────────────────────────
        // ListExpensesUseCase
        // ──────────────────────────────────────────

        [Fact]
        public void List_ReturnsPaginatedResult()
        {
            var expenses = Enumerable.Range(1, 5)
                .Select(i => MakeExpense(userId: 1, id: i))
                .ToList();

            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetAll(1, null, 1, 10)).Returns(expenses);
            repo.Setup(r => r.Count(1, null)).Returns(15);

            var useCase = new ListExpensesUseCase(repo.Object);
            var result = useCase.Execute(userId: 1, category: null, page: 1, pageSize: 10);

            Assert.Equal(15, result.TotalCount);
            Assert.Equal(2, result.TotalPages);
            Assert.True(result.HasNextPage);
            Assert.False(result.HasPreviousPage);
            Assert.Equal(5, result.Items.Count());
        }
    }
}