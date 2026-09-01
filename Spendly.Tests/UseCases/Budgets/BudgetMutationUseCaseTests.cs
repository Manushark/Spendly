using Moq;
using Spendly.Application.DTOs.Budget;
using Spendly.Application.Interfaces;
using Spendly.Application.UseCases.Budgets;
using Spendly.Domain.Entities;
using Spendly.Domain.Exceptions;

namespace Spendly.Tests.UseCases.Budgets;

public class UpdateBudgetUseCaseTests
{
    private readonly Mock<IBudgetRepository> _repo = new();

    private static Budget MakeBudget(int userId = 1, int id = 10)
    {
        var b = Budget.Create(userId, "Food & Dining", 500m, 2026, 7);
        typeof(Budget).GetProperty(nameof(Budget.Id))!.SetValue(b, id);
        return b;
    }

    [Fact]
    public async Task ExecuteAsync_Should_UpdateBudget_When_OwnerAndDataAreValid()
    {
        // Arrange
        var budget = MakeBudget(userId: 1, id: 10);

        _repo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(budget);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<Budget>())).Returns(Task.CompletedTask);

        var dto = new UpdateBudgetDto { Category = "Shopping", MonthlyLimit = 800m, Year = 2026, Month = 8 };

        // Act
        await new UpdateBudgetUseCase(_repo.Object).ExecuteAsync(userId: 1, id: 10, dto);

        // Assert
        _repo.Verify(r => r.UpdateAsync(It.IsAny<Budget>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Should_ThrowInvalidDomain_When_BudgetNotFound()
    {
        // Arrange
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Budget?)null);

        var dto = new UpdateBudgetDto { Category = "Food", MonthlyLimit = 100m, Year = 2026, Month = 7 };

        // Act + Assert
        await Assert.ThrowsAsync<InvalidDomainException>(() =>
            new UpdateBudgetUseCase(_repo.Object).ExecuteAsync(userId: 1, id: 99, dto));
    }

    [Fact]
    public async Task ExecuteAsync_Should_ThrowUnauthorized_When_BudgetBelongsToDifferentUser()
    {
        // Arrange — el presupuesto pertenece al usuario 1, pero quien llama es el usuario 2
        var budget = MakeBudget(userId: 1, id: 10);

        _repo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(budget);

        var dto = new UpdateBudgetDto { Category = "Shopping", MonthlyLimit = 100m, Year = 2026, Month = 7 };

        // Act + Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            new UpdateBudgetUseCase(_repo.Object).ExecuteAsync(userId: 2, id: 10, dto));
    }
}

public class DeleteBudgetUseCaseTests
{
    private readonly Mock<IBudgetRepository> _repo = new();

    private static Budget MakeBudget(int userId = 1, int id = 10)
    {
        var b = Budget.Create(userId, "Food & Dining", 500m, 2026, 7);
        typeof(Budget).GetProperty(nameof(Budget.Id))!.SetValue(b, id);
        return b;
    }

    [Fact]
    public async Task ExecuteAsync_Should_DeleteBudget_When_OwnerCalls()
    {
        // Arrange
        var budget = MakeBudget(userId: 1, id: 10);

        _repo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(budget);
        _repo.Setup(r => r.DeleteAsync(10)).ReturnsAsync(true);

        // Act
        var result = await new DeleteBudgetUseCase(_repo.Object).ExecuteAsync(userId: 1, id: 10);

        // Assert
        Assert.True(result);
        _repo.Verify(r => r.DeleteAsync(10), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Should_ReturnFalse_When_BudgetNotFound()
    {
        // Arrange
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Budget?)null);

        // Act
        var result = await new DeleteBudgetUseCase(_repo.Object).ExecuteAsync(userId: 1, id: 99);

        // Assert — devuelve false sin lanzar excepción
        Assert.False(result);
    }

    [Fact]
    public async Task ExecuteAsync_Should_ThrowUnauthorized_When_DifferentUserTriesToDelete()
    {
        // Arrange
        var budget = MakeBudget(userId: 1, id: 10);
        _repo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(budget);

        // Act + Assert — el usuario 2 NO puede borrar el presupuesto del usuario 1
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            new DeleteBudgetUseCase(_repo.Object).ExecuteAsync(userId: 2, id: 10));
    }
}
