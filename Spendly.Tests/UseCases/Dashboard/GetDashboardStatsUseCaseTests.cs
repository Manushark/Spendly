using Moq;
using Spendly.Application.Interfaces;
using Spendly.Application.UseCase.Dashboard;
using Spendly.Domain.Entities;
using Spendly.Domain.ValueObjects;

namespace Spendly.Tests.UseCases.Dashboard;

public class GetDashboardStatsUseCaseTests
{
    private readonly Mock<IExpenseRepository> _expRepo    = new();
    private readonly Mock<IIncomeRepository>  _incomeRepo = new();
    private readonly Mock<IDateTimeProvider>  _dateTime   = new();

    private static readonly DateTime FixedNow = new(2026, 7, 15);

    private static Expense MakeExpense(int userId, string category, decimal amount, DateTime date)
    {
        var money = Money.FromDecimal(amount);
        return Expense.Create(userId, money, "desc", date, category);
    }

    private void SetupBaseDate() =>
        _dateTime.Setup(d => d.Now(It.IsAny<string?>())).Returns(FixedNow);

    private void SetupEmptyExpenses() =>
        _expRepo.Setup(r => r.GetByDateRangeAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<Expense>());

    private void SetupZeroIncome() =>
        _incomeRepo.Setup(r => r.GetTotalAmountAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                   .ReturnsAsync(0m);

    // ── Happy path ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Execute_Should_ReturnCorrectTotals_For_CurrentMonth()
    {
        SetupBaseDate();

        var currentExpenses = new List<Expense>
        {
            MakeExpense(1, "Food",      300m, new DateTime(2026, 7, 5)),
            MakeExpense(1, "Transport", 100m, new DateTime(2026, 7, 10))
        };
        var prevExpenses = new List<Expense>
        {
            MakeExpense(1, "Food", 200m, new DateTime(2026, 6, 10))
        };

        // Current month (July 2026)
        _expRepo.Setup(r => r.GetByDateRangeAsync(1,
            new DateTime(2026, 7, 1),
            new DateTime(2026, 7, 31, 23, 59, 59)))
            .ReturnsAsync(currentExpenses);

        // Previous month (June 2026)
        _expRepo.Setup(r => r.GetByDateRangeAsync(1,
            new DateTime(2026, 6, 1),
            new DateTime(2026, 6, 30, 23, 59, 59)))
            .ReturnsAsync(prevExpenses);

        // Last 30 days (catch-all)
        _expRepo.Setup(r => r.GetByDateRangeAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(currentExpenses);

        _incomeRepo.Setup(r => r.GetTotalAmountAsync(1,
            new DateTime(2026, 7, 1),
            new DateTime(2026, 7, 31, 23, 59, 59)))
            .ReturnsAsync(2000m);

        var stats = await new GetDashboardStatsUseCase(_expRepo.Object, _incomeRepo.Object, _dateTime.Object)
            .ExecuteAsync(1);

        Assert.Equal(400m, stats.CurrentMonthTotal);
        Assert.Equal(2000m, stats.CurrentMonthIncome);
        Assert.Equal(1600m, stats.MonthlyBalance);
        Assert.Equal(80m, stats.SavingsRate);
        Assert.Equal("Food", stats.TopCategory);
    }

    [Fact]
    public async Task Execute_Should_ReturnZeroStats_When_NoExpenses()
    {
        SetupBaseDate();
        SetupEmptyExpenses();
        SetupZeroIncome();

        var stats = await new GetDashboardStatsUseCase(_expRepo.Object, _incomeRepo.Object, _dateTime.Object)
            .ExecuteAsync(1);

        Assert.Equal(0m, stats.CurrentMonthTotal);
        Assert.Equal(0m, stats.SavingsRate);
        Assert.Equal("N/A", stats.TopCategory);
        Assert.Empty(stats.CategoryBreakdown);
    }

    [Fact]
    public async Task Execute_Should_ReturnAtMost5TopExpenses()
    {
        SetupBaseDate();

        var expenses = Enumerable.Range(1, 8)
            .Select(i => MakeExpense(1, "Food", i * 100m, new DateTime(2026, 7, i)))
            .ToList();

        _expRepo.Setup(r => r.GetByDateRangeAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(expenses);
        _incomeRepo.Setup(r => r.GetTotalAmountAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                   .ReturnsAsync(5000m);

        var stats = await new GetDashboardStatsUseCase(_expRepo.Object, _incomeRepo.Object, _dateTime.Object)
            .ExecuteAsync(1);

        var topList = stats.TopExpenses.ToList();
        Assert.True(topList.Count <= 5);
        // Verify descending order
        for (int i = 0; i < topList.Count - 1; i++)
            Assert.True(topList[i].Amount >= topList[i + 1].Amount);
    }

    [Fact]
    public async Task Execute_Should_CalculateSavingsRate_As_Zero_When_NoIncome()
    {
        SetupBaseDate();
        SetupEmptyExpenses();
        SetupZeroIncome();

        var stats = await new GetDashboardStatsUseCase(_expRepo.Object, _incomeRepo.Object, _dateTime.Object)
            .ExecuteAsync(1);

        // No division by zero — savings rate should be 0
        Assert.Equal(0m, stats.SavingsRate);
    }
}
