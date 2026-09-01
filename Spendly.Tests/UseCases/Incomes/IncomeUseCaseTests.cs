using Moq;
using Spendly.Application.DTOs.Income;
using Spendly.Application.Interfaces;
using Spendly.Application.UseCases.Incomes;
using Spendly.Domain.Entities;
using Spendly.Domain.Exceptions;

namespace Spendly.Tests.UseCases.Incomes;

public class CreateIncomeUseCaseTests
{
    private readonly Mock<IIncomeRepository> _repo = new();

    private static CreateIncomeDto ValidDto() => new()
    {
        Amount      = 3000m,
        Currency    = "USD",
        Source      = "Salary",
        Description = "Monthly salary",
        Date        = DateTime.Today,
        IsRecurring = true
    };

    [Fact]
    public async Task ExecuteAsync_Should_CreateIncome_When_DataIsValid()
    {
        // Arrange
        _repo.Setup(r => r.AddAsync(It.IsAny<Income>())).Returns(Task.CompletedTask);

        // Act
        await new CreateIncomeUseCase(_repo.Object).ExecuteAsync(userId: 1, ValidDto());

        // Assert — se guardó exactamente un ingreso
        _repo.Verify(r => r.AddAsync(It.IsAny<Income>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Should_ThrowInvalidDomain_When_AmountIsZeroOrNegative()
    {
        // Arrange
        var dto = ValidDto();
        dto.Amount = 0m;

        // Act + Assert — la validación ocurre en el Domain (Income.Create)
        await Assert.ThrowsAsync<InvalidDomainException>(() =>
            new CreateIncomeUseCase(_repo.Object).ExecuteAsync(userId: 1, dto));
    }

    [Fact]
    public async Task ExecuteAsync_Should_ThrowInvalidDomain_When_SourceIsEmpty()
    {
        // Arrange
        var dto = ValidDto();
        dto.Source = "";

        // Act + Assert
        await Assert.ThrowsAsync<InvalidDomainException>(() =>
            new CreateIncomeUseCase(_repo.Object).ExecuteAsync(userId: 1, dto));
    }

    [Fact]
    public async Task ExecuteAsync_Should_ThrowInvalidDomain_When_DateIsInTheFuture()
    {
        // Arrange
        var dto = ValidDto();
        dto.Date = DateTime.UtcNow.AddDays(30); // 30 días en el futuro

        // Act + Assert
        await Assert.ThrowsAsync<InvalidDomainException>(() =>
            new CreateIncomeUseCase(_repo.Object).ExecuteAsync(userId: 1, dto));
    }
}

public class UpdateIncomeUseCaseTests
{
    private readonly Mock<IIncomeRepository> _repo = new();

    private static Income MakeIncome(int userId = 1, int id = 5)
    {
        var income = Income.Create(userId, 2000m, "USD", "Freelance", null, DateTime.Today);
        typeof(Income).GetProperty(nameof(Income.Id))!.SetValue(income, id);
        return income;
    }

    [Fact]
    public async Task ExecuteAsync_Should_UpdateIncome_When_OwnerAndDataAreValid()
    {
        // Arrange
        var income = MakeIncome(userId: 1, id: 5);

        _repo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(income);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<Income>())).Returns(Task.CompletedTask);

        var dto = new UpdateIncomeDto
        {
            Amount = 2500m, Currency = "USD", Source = "Consulting",
            Description = null, Date = DateTime.Today, IsRecurring = false
        };

        // Act
        await new UpdateIncomeUseCase(_repo.Object).ExecuteAsync(userId: 1, incomeId: 5, dto);

        // Assert
        _repo.Verify(r => r.UpdateAsync(It.IsAny<Income>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Should_ThrowKeyNotFound_When_IncomeDoesNotExist()
    {
        // Arrange
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Income?)null);

        var dto = new UpdateIncomeDto
        {
            Amount = 100m, Currency = "USD", Source = "X",
            Date = DateTime.Today, IsRecurring = false
        };

        // Act + Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            new UpdateIncomeUseCase(_repo.Object).ExecuteAsync(userId: 1, incomeId: 99, dto));
    }

    [Fact]
    public async Task ExecuteAsync_Should_ThrowUnauthorized_When_IncomeBelongsToDifferentUser()
    {
        // Arrange
        var income = MakeIncome(userId: 1, id: 5);
        _repo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(income);

        var dto = new UpdateIncomeDto
        {
            Amount = 100m, Currency = "USD", Source = "X",
            Date = DateTime.Today, IsRecurring = false
        };

        // Act + Assert — usuario 2 intenta editar el ingreso del usuario 1
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            new UpdateIncomeUseCase(_repo.Object).ExecuteAsync(userId: 2, incomeId: 5, dto));
    }
}
