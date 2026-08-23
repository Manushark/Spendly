using Moq;
using Spendly.Application.DTOs.SavingsGoal;
using Spendly.Application.Interfaces;
using Spendly.Application.UseCases.SavingsGoals;
using Spendly.Domain.Entities;
using Spendly.Domain.Enums;

namespace Spendly.Tests.UseCases.SavingsGoals;

/// <summary>
/// Tests adicionales para CreateSavingsGoalUseCase, UpdateSavingsGoalUseCase,
/// ListSavingsGoalsUseCase, GetSavingsGoalByIdUseCase y AddFundsUseCase (milestone + completado).
/// AddFundsUseCase básico y DeleteSavingsGoalUseCase ya están en archivos existentes.
/// </summary>
public class SavingsGoalExtendedTests
{
    private readonly Mock<ISavingsGoalRepository> _repo = new();
    private readonly Mock<INotificationRepository> _notifRepo = new();

    private static SavingsGoal MakeGoal(int userId = 1, int id = 1,
        decimal target = 1000m, decimal current = 0m)
    {
        var g = SavingsGoal.Create(userId, "Vacation Fund", target, current,
            DateTime.Today.AddYears(1), "✈️", "#6366F1");
        typeof(SavingsGoal).GetProperty(nameof(SavingsGoal.Id))!.SetValue(g, id);
        return g;
    }

    // ── CreateSavingsGoalUseCase ──────────────────────────────────────────────────

    [Fact]
    public async Task Create_Should_AddGoal_When_DataIsValid()
    {
        _repo.Setup(r => r.AddAsync(It.IsAny<SavingsGoal>())).Returns(Task.CompletedTask);

        var dto = new CreateSavingsGoalDto
        {
            Name = "Emergency Fund",
            TargetAmount = 5000m,
            CurrentAmount = 0m,
            Deadline = DateTime.Today.AddYears(1),
            Icon = "🛡️",
            Color = "#10B981"
        };

        await new CreateSavingsGoalUseCase(_repo.Object).ExecuteAsync(1, dto);

        _repo.Verify(r => r.AddAsync(It.IsAny<SavingsGoal>()), Times.Once);
    }

    // ── UpdateSavingsGoalUseCase ──────────────────────────────────────────────────

    [Fact]
    public async Task Update_Should_UpdateGoal_When_OwnerAndDataAreValid()
    {
        var goal = MakeGoal(userId: 1, id: 1);
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(goal);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<SavingsGoal>())).Returns(Task.CompletedTask);

        var dto = new UpdateSavingsGoalDto
        {
            Name = "New Car",
            TargetAmount = 20000m,
            CurrentAmount = 1000m,
            Deadline = DateTime.Today.AddYears(2),
            Icon = "🚗",
            Color = "#F59E0B"
        };

        await new UpdateSavingsGoalUseCase(_repo.Object).ExecuteAsync(1, 1, dto);

        _repo.Verify(r => r.UpdateAsync(It.IsAny<SavingsGoal>()), Times.Once);
    }

    [Fact]
    public async Task Update_Should_ThrowKeyNotFound_When_GoalNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((SavingsGoal?)null);

        var dto = new UpdateSavingsGoalDto { Name = "X", TargetAmount = 1m, CurrentAmount = 0m,
            Deadline = DateTime.Today.AddYears(1), Icon = "x", Color = "#fff" };

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            new UpdateSavingsGoalUseCase(_repo.Object).ExecuteAsync(1, 99, dto));
    }

    [Fact]
    public async Task Update_Should_ThrowUnauthorized_When_WrongUser()
    {
        var goal = MakeGoal(userId: 1, id: 1);
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(goal);

        var dto = new UpdateSavingsGoalDto { Name = "X", TargetAmount = 1m, CurrentAmount = 0m,
            Deadline = DateTime.Today.AddYears(1), Icon = "x", Color = "#fff" };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            new UpdateSavingsGoalUseCase(_repo.Object).ExecuteAsync(2, 1, dto));
    }

    // ── ListSavingsGoalsUseCase ───────────────────────────────────────────────────

    [Fact]
    public async Task List_Should_ReturnMappedDtos()
    {
        var goals = new List<SavingsGoal> { MakeGoal(id: 1), MakeGoal(id: 2, target: 500m, current: 250m) };
        _repo.Setup(r => r.GetAllByUserAsync(1)).ReturnsAsync(goals);

        var result = await new ListSavingsGoalsUseCase(_repo.Object).ExecuteAsync(1);

        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Equal("Vacation Fund", r.Name));
    }

    [Fact]
    public async Task List_Should_ReturnEmpty_When_NoGoals()
    {
        _repo.Setup(r => r.GetAllByUserAsync(1)).ReturnsAsync(new List<SavingsGoal>());

        var result = await new ListSavingsGoalsUseCase(_repo.Object).ExecuteAsync(1);

        Assert.Empty(result);
    }

    // ── GetSavingsGoalByIdUseCase ─────────────────────────────────────────────────

    [Fact]
    public async Task GetById_Should_ReturnDto_When_OwnerAndFound()
    {
        var goal = MakeGoal(userId: 1, id: 1, target: 1000m, current: 300m);
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(goal);

        var result = await new GetSavingsGoalByIdUseCase(_repo.Object).ExecuteAsync(1, 1);

        Assert.NotNull(result);
        Assert.Equal("Vacation Fund", result!.Name);
        Assert.Equal(30m, result.ProgressPercentage);
    }

    [Fact]
    public async Task GetById_Should_ReturnNull_When_NotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((SavingsGoal?)null);

        var result = await new GetSavingsGoalByIdUseCase(_repo.Object).ExecuteAsync(1, 99);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetById_Should_ThrowUnauthorized_When_WrongUser()
    {
        var goal = MakeGoal(userId: 1, id: 1);
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(goal);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            new GetSavingsGoalByIdUseCase(_repo.Object).ExecuteAsync(2, 1));
    }

    // ── AddFundsUseCase — Milestone & Completion notifications ────────────────────

    [Fact]
    public async Task AddFunds_Should_CreateCompletedNotification_When_GoalReaches100Percent()
    {
        // Goal: target 1000, current 900 → add 200 → crosses 100%
        var goal = MakeGoal(userId: 1, id: 1, target: 1000m, current: 900m);
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(goal);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<SavingsGoal>())).Returns(Task.CompletedTask);
        _notifRepo.Setup(r => r.AddAsync(It.IsAny<Notification>())).Returns(Task.CompletedTask);

        await new AddFundsUseCase(_repo.Object, _notifRepo.Object).ExecuteAsync(1, 1, 200m);

        _notifRepo.Verify(r => r.AddAsync(
            It.Is<Notification>(n => n.Type == NotificationType.SavingsGoalCompleted)),
            Times.Once);
    }

    [Fact]
    public async Task AddFunds_Should_CreateMilestoneNotification_When_GoalReaches50Percent()
    {
        // Goal: target 1000, current 400 → add 200 → crosses 50%
        var goal = MakeGoal(userId: 1, id: 1, target: 1000m, current: 400m);
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(goal);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<SavingsGoal>())).Returns(Task.CompletedTask);
        _notifRepo.Setup(r => r.AddAsync(It.IsAny<Notification>())).Returns(Task.CompletedTask);

        await new AddFundsUseCase(_repo.Object, _notifRepo.Object).ExecuteAsync(1, 1, 200m);

        _notifRepo.Verify(r => r.AddAsync(
            It.Is<Notification>(n => n.Type == NotificationType.SavingsGoalMilestone)),
            Times.Once);
    }

    [Fact]
    public async Task AddFunds_Should_NotCreateNotification_When_ProgressBelow50Percent()
    {
        // Goal: target 1000, current 100 → add 100 → still below 50%
        var goal = MakeGoal(userId: 1, id: 1, target: 1000m, current: 100m);
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(goal);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<SavingsGoal>())).Returns(Task.CompletedTask);

        await new AddFundsUseCase(_repo.Object, _notifRepo.Object).ExecuteAsync(1, 1, 100m);

        _notifRepo.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Never);
    }
}
