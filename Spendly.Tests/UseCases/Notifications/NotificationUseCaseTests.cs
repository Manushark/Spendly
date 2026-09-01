using Moq;
using Spendly.Application.Interfaces;
using Spendly.Application.UseCases.Notifications;
using Spendly.Domain.Entities;
using Spendly.Domain.Enums;

namespace Spendly.Tests.UseCases.Notifications;

public class NotificationUseCaseTests
{
    private readonly Mock<INotificationRepository> _repo = new();

    private static Notification MakeNotification(int userId = 1, int id = 10)
    {
        var n = Notification.Create(userId, "Test message", NotificationType.BudgetWarning, 99);
        typeof(Notification).GetProperty(nameof(Notification.Id))!.SetValue(n, id);
        return n;
    }

    // ── GetNotificationsUseCase ──────────────────────────────────────────────────

    [Fact]
    public async Task GetNotifications_Should_ReturnMappedDtos()
    {
        _repo.Setup(r => r.GetAllByUserAsync(1, 1, 20))
             .ReturnsAsync(new List<Notification> { MakeNotification() });

        var result = await new GetNotificationsUseCase(_repo.Object).ExecuteAsync(1);

        Assert.Single(result);
        Assert.Equal("Test message", result[0].Message);
        Assert.Equal("BudgetWarning", result[0].Type);
    }

    [Fact]
    public async Task GetNotifications_Should_ReturnEmpty_When_NoNotifications()
    {
        _repo.Setup(r => r.GetAllByUserAsync(1, 1, 20))
             .ReturnsAsync(new List<Notification>());

        var result = await new GetNotificationsUseCase(_repo.Object).ExecuteAsync(1);

        Assert.Empty(result);
    }

    // ── MarkNotificationReadUseCase ──────────────────────────────────────────────

    [Fact]
    public async Task MarkRead_Should_CallRepository()
    {
        _repo.Setup(r => r.MarkAsReadAsync(1, 10)).Returns(Task.CompletedTask);

        await new MarkNotificationReadUseCase(_repo.Object).ExecuteAsync(userId: 1, id: 10);

        _repo.Verify(r => r.MarkAsReadAsync(1, 10), Times.Once);
    }

    // ── MarkAllNotificationsReadUseCase ──────────────────────────────────────────

    [Fact]
    public async Task MarkAllRead_Should_CallRepositoryMarkAll()
    {
        _repo.Setup(r => r.MarkAllAsReadAsync(1)).Returns(Task.CompletedTask);

        await new MarkAllNotificationsReadUseCase(_repo.Object).ExecuteAsync(userId: 1);

        _repo.Verify(r => r.MarkAllAsReadAsync(1), Times.Once);
    }

    // ── GetUnreadCountUseCase ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetUnreadCount_Should_ReturnCount()
    {
        _repo.Setup(r => r.CountUnreadAsync(1)).ReturnsAsync(5);

        var count = await new GetUnreadCountUseCase(_repo.Object).ExecuteAsync(userId: 1);

        Assert.Equal(5, count);
    }

    [Fact]
    public async Task GetUnreadCount_Should_ReturnZero_When_AllRead()
    {
        _repo.Setup(r => r.CountUnreadAsync(1)).ReturnsAsync(0);

        var count = await new GetUnreadCountUseCase(_repo.Object).ExecuteAsync(userId: 1);

        Assert.Equal(0, count);
    }

    // ── DeleteNotificationUseCase ─────────────────────────────────────────────────

    [Fact]
    public async Task DeleteNotification_Should_CallRepository()
    {
        _repo.Setup(r => r.DeleteAsync(1, 10)).Returns(Task.CompletedTask);

        await new DeleteNotificationUseCase(_repo.Object).ExecuteAsync(userId: 1, id: 10);

        _repo.Verify(r => r.DeleteAsync(1, 10), Times.Once);
    }

    // ── DeleteAllNotificationsUseCase ─────────────────────────────────────────────

    [Fact]
    public async Task DeleteAllNotifications_Should_CallRepositoryDeleteAll()
    {
        _repo.Setup(r => r.DeleteAllAsync(1)).Returns(Task.CompletedTask);

        await new DeleteAllNotificationsUseCase(_repo.Object).ExecuteAsync(userId: 1);

        _repo.Verify(r => r.DeleteAllAsync(1), Times.Once);
    }
}
