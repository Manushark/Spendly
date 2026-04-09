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
        public async Task Create_ValidDto_CallsRepositoryAdd()
        {
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.AddAsync(It.IsAny<Expense>())).Returns(Task.CompletedTask);
            var useCase = new CreateExpenseUseCase(repo.Object);

            var dto = new CreateExpenseDto
            {
                Amount = 100m,
                Description = "Groceries",
                Date = DateTime.UtcNow.AddDays(-1),
                Category = "Food"
            };

            await useCase.ExecuteAsync(userId: 1, dto);

            repo.Verify(r => r.AddAsync(It.Is<Expense>(e =>
                e.UserId == 1 &&
                e.Amount.Value == 100m &&
                e.Description == "Groceries"
            )), Times.Once);
        }

        [Fact]
        public async Task Create_FutureDate_ThrowsDomainException()
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

            await Assert.ThrowsAsync<InvalidDomainException>(() => useCase.ExecuteAsync(1, dto));
            repo.Verify(r => r.AddAsync(It.IsAny<Expense>()), Times.Never);
        }

        [Fact]
        public async Task Create_NegativeAmount_ThrowsArgumentException()
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

            await Assert.ThrowsAsync<ArgumentException>(() => useCase.ExecuteAsync(1, dto));
        }

        // ──────────────────────────────────────────
        // UpdateExpenseUseCase
        // ──────────────────────────────────────────

        [Fact]
        public async Task Update_OwnExpense_CallsRepositoryUpdate()
        {
            var expense = MakeExpense(userId: 1, id: 10);
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(expense);
            repo.Setup(r => r.UpdateAsync(It.IsAny<Expense>())).Returns(Task.CompletedTask);

            var useCase = new UpdateExpenseUseCase(repo.Object);

            var dto = new UpdateExpenseDto
            {
                Amount = 200m,
                Description = "Updated",
                Date = DateTime.UtcNow.AddDays(-2),
                Category = "Transport"
            };

            await useCase.ExecuteAsync(userId: 1, id: 10, dto);

            repo.Verify(r => r.UpdateAsync(expense), Times.Once);
        }

        [Fact]
        public async Task Update_OtherUserExpense_ThrowsUnauthorized()
        {
            var expense = MakeExpense(userId: 1, id: 10);  // pertenece a user 1
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(expense);

            var useCase = new UpdateExpenseUseCase(repo.Object);

            var dto = new UpdateExpenseDto
            {
                Amount = 200m,
                Description = "Hack",
                Date = DateTime.UtcNow.AddDays(-1),
                Category = "Other"
            };

            // user 2 intenta modificar el gasto de user 1
            await Assert.ThrowsAsync<UnauthorizedExpenseAccessException>(() =>
                useCase.ExecuteAsync(userId: 2, id: 10, dto));

            repo.Verify(r => r.UpdateAsync(It.IsAny<Expense>()), Times.Never);
        }

        [Fact]
        public async Task Update_NotFound_ThrowsExpenseNotFoundException()
        {
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Expense?)null);

            var useCase = new UpdateExpenseUseCase(repo.Object);

            await Assert.ThrowsAsync<ExpenseNotFoundException>(() =>
                useCase.ExecuteAsync(1, 99, new UpdateExpenseDto
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
        public async Task Delete_OwnExpense_ReturnsTrue()
        {
            var expense = MakeExpense(userId: 1, id: 5);
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(expense);
            repo.Setup(r => r.DeleteAsync(5)).ReturnsAsync(true);

            var useCase = new DeleteExpenseUseCase(repo.Object);
            var result = await useCase.ExecuteAsync(userId: 1, id: 5);

            Assert.True(result);
        }

        [Fact]
        public async Task Delete_OtherUserExpense_ThrowsUnauthorized()
        {
            var expense = MakeExpense(userId: 1, id: 5);
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(expense);

            var useCase = new DeleteExpenseUseCase(repo.Object);

            await Assert.ThrowsAsync<UnauthorizedExpenseAccessException>(() =>
                useCase.ExecuteAsync(userId: 2, id: 5));

            repo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        // ──────────────────────────────────────────
        // GetExpenseByIdUseCase
        // ──────────────────────────────────────────

        [Fact]
        public async Task GetById_OwnExpense_ReturnsDto()
        {
            var expense = MakeExpense(userId: 1, id: 3);
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(expense);

            var useCase = new GetExpenseByIdUseCase(repo.Object);
            var result = await useCase.ExecuteAsync(userId: 1, id: 3);

            Assert.NotNull(result);
            Assert.Equal("Test expense", result!.Description);
        }

        [Fact]
        public async Task GetById_NotFound_ReturnsNull()
        {
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Expense?)null);

            var useCase = new GetExpenseByIdUseCase(repo.Object);
            var result = await useCase.ExecuteAsync(userId: 1, id: 99);

            Assert.Null(result);
        }

        // ──────────────────────────────────────────
        // ListExpensesUseCase
        // ──────────────────────────────────────────

        [Fact]
        public async Task List_ReturnsPaginatedResult()
        {
            var expenses = Enumerable.Range(1, 5)
                .Select(i => MakeExpense(userId: 1, id: i))
                .ToList();

            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetAllAsync(1, null, 1, 10)).ReturnsAsync(expenses);
            repo.Setup(r => r.CountAsync(1, null)).ReturnsAsync(15);

            var useCase = new ListExpensesUseCase(repo.Object);
            var result = await useCase.ExecuteAsync(userId: 1, category: null, page: 1, pageSize: 10);

            Assert.Equal(15, result.TotalCount);
            Assert.Equal(2, result.TotalPages);
            Assert.True(result.HasNextPage);
            Assert.False(result.HasPreviousPage);
            Assert.Equal(5, result.Items.Count());
        }
    }
}