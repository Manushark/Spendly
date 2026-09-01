using Moq;
using Spendly.Application.DTOs.RecurringExpense;
using Spendly.Application.Interfaces;
using Spendly.Application.Services;
using Spendly.Application.UseCases.RecurringExpenses;
using Spendly.Domain.Entities;
using Spendly.Domain.Enums;
using Spendly.Domain.Exceptions;

namespace Spendly.Tests.UseCases.RecurringExpenses;

public class RecurringExpenseUseCaseTests
{
    private readonly Mock<IRecurringExpenseRepository> _repo = new();

    private static RecurringExpense MakeRecurring(int userId = 1, int id = 10)
    {
        var r = RecurringExpense.Create(userId, "Netflix", 15m, "Entertainment",
            RecurrenceFrequency.Monthly, DateTime.Today, null);
        typeof(RecurringExpense).GetProperty(nameof(RecurringExpense.Id))!.SetValue(r, id);
        return r;
    }

    // ── CreateRecurringExpenseUseCase ────────────────────────────────────────────

    [Fact]
    public async Task Create_Should_AddRecurringExpense_When_DataIsValid()
    {
        _repo.Setup(r => r.AddAsync(It.IsAny<RecurringExpense>())).Returns(Task.CompletedTask);

        var dto = new CreateRecurringExpenseDto
        {
            Description = "Netflix",
            Amount = 15m,
            Category = "Entertainment",
            Frequency = RecurrenceFrequency.Monthly,
            StartDate = DateTime.Today
        };

        await new CreateRecurringExpenseUseCase(_repo.Object).ExecuteAsync(1, dto);

        _repo.Verify(r => r.AddAsync(It.IsAny<RecurringExpense>()), Times.Once);
    }

    // ── UpdateRecurringExpenseUseCase ────────────────────────────────────────────

    [Fact]
    public async Task Update_Should_UpdateRecurring_When_OwnerAndDataAreValid()
    {
        var recurring = MakeRecurring(userId: 1, id: 10);
        _repo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(recurring);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<RecurringExpense>())).Returns(Task.CompletedTask);

        var dto = new UpdateRecurringExpenseDto
        {
            Description = "Netflix Premium",
            Amount = 20m,
            Category = "Entertainment",
            Frequency = RecurrenceFrequency.Monthly,
            StartDate = DateTime.Today
        };

        await new UpdateRecurringExpenseUseCase(_repo.Object).ExecuteAsync(1, 10, dto);

        _repo.Verify(r => r.UpdateAsync(It.IsAny<RecurringExpense>()), Times.Once);
    }

    [Fact]
    public async Task Update_Should_Throw_When_RecurringNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((RecurringExpense?)null);

        var dto = new UpdateRecurringExpenseDto
        {
            Description = "X", Amount = 1m, Category = "Y",
            Frequency = RecurrenceFrequency.Monthly, StartDate = DateTime.Today
        };

        await Assert.ThrowsAsync<InvalidDomainException>(() =>
            new UpdateRecurringExpenseUseCase(_repo.Object).ExecuteAsync(1, 99, dto));
    }

    [Fact]
    public async Task Update_Should_ThrowUnauthorized_When_WrongUser()
    {
        var recurring = MakeRecurring(userId: 1, id: 10);
        _repo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(recurring);

        var dto = new UpdateRecurringExpenseDto
        {
            Description = "X", Amount = 1m, Category = "Y",
            Frequency = RecurrenceFrequency.Monthly, StartDate = DateTime.Today
        };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            new UpdateRecurringExpenseUseCase(_repo.Object).ExecuteAsync(2, 10, dto));
    }

    // ── DeleteRecurringExpenseUseCase ────────────────────────────────────────────

    [Fact]
    public async Task Delete_Should_ReturnTrue_When_Deleted()
    {
        var recurring = MakeRecurring(userId: 1, id: 10);
        _repo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(recurring);
        _repo.Setup(r => r.DeleteAsync(10)).ReturnsAsync(true);

        var result = await new DeleteRecurringExpenseUseCase(_repo.Object).ExecuteAsync(1, 10);

        Assert.True(result);
    }

    [Fact]
    public async Task Delete_Should_ReturnFalse_When_NotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((RecurringExpense?)null);

        var result = await new DeleteRecurringExpenseUseCase(_repo.Object).ExecuteAsync(1, 99);

        Assert.False(result);
    }

    [Fact]
    public async Task Delete_Should_ThrowUnauthorized_When_WrongUser()
    {
        var recurring = MakeRecurring(userId: 1, id: 10);
        _repo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(recurring);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            new DeleteRecurringExpenseUseCase(_repo.Object).ExecuteAsync(2, 10));
    }

    // ── ToggleRecurringExpenseUseCase ────────────────────────────────────────────

    [Fact]
    public async Task Toggle_Should_DeactivateRecurring_When_ActivateIsFalse()
    {
        var recurring = MakeRecurring(userId: 1, id: 10);
        _repo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(recurring);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<RecurringExpense>())).Returns(Task.CompletedTask);

        await new ToggleRecurringExpenseUseCase(_repo.Object).ExecuteAsync(1, 10, activate: false);

        Assert.False(recurring.IsActive);
        _repo.Verify(r => r.UpdateAsync(It.IsAny<RecurringExpense>()), Times.Once);
    }

    [Fact]
    public async Task Toggle_Should_ActivateRecurring_When_ActivateIsTrue()
    {
        var recurring = MakeRecurring(userId: 1, id: 10);
        // Deactivate first manually via reflection to simulate an inactive state
        typeof(RecurringExpense).GetProperty(nameof(RecurringExpense.IsActive))!.SetValue(recurring, false);

        _repo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(recurring);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<RecurringExpense>())).Returns(Task.CompletedTask);

        await new ToggleRecurringExpenseUseCase(_repo.Object).ExecuteAsync(1, 10, activate: true);

        Assert.True(recurring.IsActive);
    }

    [Fact]
    public async Task Toggle_Should_Throw_When_NotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((RecurringExpense?)null);

        await Assert.ThrowsAsync<InvalidDomainException>(() =>
            new ToggleRecurringExpenseUseCase(_repo.Object).ExecuteAsync(1, 99, activate: false));
    }

    // ── GetRecurringExpenseByIdUseCase ───────────────────────────────────────────

    [Fact]
    public async Task GetById_Should_ReturnDto_When_Found()
    {
        var recurring = MakeRecurring(userId: 1, id: 10);
        _repo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(recurring);

        var result = await new GetRecurringExpenseByIdUseCase(_repo.Object).ExecuteAsync(1, 10);

        Assert.NotNull(result);
        Assert.Equal("Netflix", result!.Description);
        Assert.Equal(15m, result.Amount);
    }

    [Fact]
    public async Task GetById_Should_ReturnNull_When_NotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((RecurringExpense?)null);

        var result = await new GetRecurringExpenseByIdUseCase(_repo.Object).ExecuteAsync(1, 99);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetById_Should_ThrowUnauthorized_When_WrongUser()
    {
        var recurring = MakeRecurring(userId: 1, id: 10);
        _repo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(recurring);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            new GetRecurringExpenseByIdUseCase(_repo.Object).ExecuteAsync(2, 10));
    }
}
