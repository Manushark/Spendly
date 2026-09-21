using Moq;
using Spendly.Application.DTOs.Ai;
using Spendly.Application.DTOs.Budget;
using Spendly.Application.DTOs.Expense;
using Spendly.Application.Interfaces;
using Spendly.Application.UseCase.CreateExpense;
using Spendly.Application.UseCases.Ai;
using Spendly.Application.UseCases.Budgets;
using Spendly.Domain.Entities;
using Spendly.Domain.Exceptions;

namespace Spendly.Tests.UseCases.Ai
{
    public class AiUseCasesTests
    {
        private readonly Mock<IAiAssistantService> _aiService = new();
        private readonly Mock<ICategoryRepository> _categoryRepo = new();
        private readonly Mock<IUserRepository> _userRepo = new();
        private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();

        private readonly Mock<IExpenseRepository> _expenseRepo = new();
        private readonly Mock<IBudgetRepository> _budgetRepo = new();
        private readonly Mock<ITagRepository> _tagRepo = new();

        [Fact]
        public async Task ParseAiCommandUseCase_Should_ThrowException_When_PromptIsEmpty()
        {
            // Arrange
            var useCase = new ParseAiCommandUseCase(_aiService.Object, _categoryRepo.Object, _userRepo.Object, _dateTimeProvider.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidDomainException>(() => useCase.ExecuteAsync(1, "   "));
        }

        [Fact]
        public async Task ParseAiCommandUseCase_Should_ForwardContext_To_AiAssistantService()
        {
            // Arrange
            const int userId = 10;
            var user = User.Create("user@spendly.com", "hash");
            user.UpdateProfile("User", "DOP", "America/Santo_Domingo");

            _userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _dateTimeProvider.Setup(d => d.Now("America/Santo_Domingo")).Returns(new DateTime(2026, 9, 16, 14, 0, 0));
            _categoryRepo.Setup(c => c.GetAllByUserAsync(userId)).ReturnsAsync(new List<Category>
            {
                Category.Create(userId, "Food & Dining", "bi-cup", "#ff0000"),
                Category.Create(userId, "Transportation", "bi-car", "#00ff00")
            });

            var expectedPlan = new AiFinancialPlanDto
            {
                Expenses = [new AiParsedExpenseDto { Amount = 600, Category = "Food & Dining", Description = "Almuerzo" }],
                Summary = "Se detectó 1 gasto."
            };

            _aiService.Setup(s => s.ParseCommandAsync(
                "Gasté 600 en almuerzo",
                It.IsAny<DateTime>(),
                "America/Santo_Domingo",
                It.Is<IEnumerable<string>>(cats => cats.Contains("Food & Dining")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPlan);

            var useCase = new ParseAiCommandUseCase(_aiService.Object, _categoryRepo.Object, _userRepo.Object, _dateTimeProvider.Object);

            // Act
            var result = await useCase.ExecuteAsync(userId, "Gasté 600 en almuerzo");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Expenses);
            Assert.Equal(600, result.Expenses[0].Amount);
            Assert.Equal("Food & Dining", result.Expenses[0].Category);
        }

        [Fact]
        public async Task ExecuteAiPlanUseCase_Should_ReturnFailure_When_PlanHasNoActions()
        {
            // Arrange
            var createExpense = new CreateExpenseUseCase(_expenseRepo.Object, null!, _tagRepo.Object);
            var createBudget = new CreateBudgetUseCase(_budgetRepo.Object);
            var useCase = new ExecuteAiPlanUseCase(createExpense, createBudget, _budgetRepo.Object, _tagRepo.Object, _userRepo.Object, _dateTimeProvider.Object);

            // Act
            var result = await useCase.ExecuteAsync(1, new AiFinancialPlanDto());

            // Assert
            Assert.False(result.Success);
            Assert.Equal(0, result.CreatedExpensesCount);
        }

        [Fact]
        public async Task ParseAiCommandUseCase_Should_Populate_AvailableCategories_On_Plan()
        {
            // Arrange
            const int userId = 10;
            var user = User.Create("user@spendly.com", "hash");
            user.UpdateProfile("User", "DOP", "America/Santo_Domingo");

            _userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _dateTimeProvider.Setup(d => d.Now("America/Santo_Domingo")).Returns(new DateTime(2026, 9, 19, 10, 0, 0));
            _categoryRepo.Setup(c => c.GetAllByUserAsync(userId)).ReturnsAsync(new List<Category>
            {
                Category.Create(userId, "Food & Dining", "bi-cup", "#ff0000"),
                Category.Create(userId, "Health", "bi-heart", "#00ff00")
            });

            _aiService.Setup(s => s.ParseCommandAsync(
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AiFinancialPlanDto());

            var useCase = new ParseAiCommandUseCase(_aiService.Object, _categoryRepo.Object, _userRepo.Object, _dateTimeProvider.Object);

            // Act
            var result = await useCase.ExecuteAsync(userId, "200 para el salón");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Food & Dining", result.AvailableCategories);
            Assert.Contains("Health", result.AvailableCategories);
        }
    }
}
