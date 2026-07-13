using Moq;
using Spendly.Application.Interfaces;
using Spendly.Application.UseCases.SavingsGoals;
using Spendly.Domain.Entities;

namespace Spendly.Tests.UseCases.SavingsGoals;

public class DeleteSavingsGoalUseCaseTests
{
    private readonly Mock<ISavingsGoalRepository> _repo = new();

    private static SavingsGoal MakeGoal(int userId = 1, int id = 10)
    {
        var goal = SavingsGoal.Create(userId, "Vacaciones", 2000m, 0m, null, "bi-suitcase", "#6366f1");
        typeof(SavingsGoal).GetProperty(nameof(SavingsGoal.Id))!.SetValue(goal, id);
        return goal;
    }

    [Fact]
    public async Task ExecuteAsync_Should_DeleteGoal_When_OwnerCalls()
    {
        // ARRANGE
        var goal = MakeGoal(userId: 1, id: 10);

        _repo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(goal);
        _repo.Setup(r => r.DeleteAsync(10)).ReturnsAsync(true);

        var useCase = new DeleteSavingsGoalUseCase(_repo.Object);

        // ACT
        var result = await useCase.ExecuteAsync(userId: 1, id: 10);

        // ASSERT
        Assert.True(result);
        _repo.Verify(r => r.DeleteAsync(10), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Should_ThrowKeyNotFound_When_GoalDoesNotExist()
    {
        // ARRANGE — el repositorio no encuentra nada
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
             .ReturnsAsync((SavingsGoal?)null);

        var useCase = new DeleteSavingsGoalUseCase(_repo.Object);

        // ACT + ASSERT
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            useCase.ExecuteAsync(userId: 1, id: 99));
    }

    [Fact]
    public async Task ExecuteAsync_Should_ThrowUnauthorized_When_GoalBelongsToDifferentUser()
    {
        // ARRANGE — el goal pertenece al usuario 1
        var goal = MakeGoal(userId: 1, id: 10);
        _repo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(goal);

        var useCase = new DeleteSavingsGoalUseCase(_repo.Object);

        // ACT + ASSERT — el usuario 2 intenta borrarlo
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            useCase.ExecuteAsync(userId: 2, id: 10));
    }
}