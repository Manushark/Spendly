using Spendly.Application.Interfaces;
using Spendly.Domain.Entities;
using Spendly.Domain.Enums;

namespace Spendly.Application.Services
{
    /// <summary>
    /// Servicio que verifica presupuestos después de crear gastos
    /// y genera notificaciones cuando se alcanza el 80% o 100%.
    /// </summary>
    public class BudgetAlertService
    {
        private readonly IBudgetRepository _budgetRepo;
        private readonly IExpenseRepository _expenseRepo;
        private readonly INotificationRepository _notificationRepo;

        public BudgetAlertService(
            IBudgetRepository budgetRepo,
            IExpenseRepository expenseRepo,
            INotificationRepository notificationRepo)
        {
            _budgetRepo = budgetRepo;
            _expenseRepo = expenseRepo;
            _notificationRepo = notificationRepo;
        }

        public async Task CheckAndCreateAlertsAsync(int userId)
        {
            var now = DateTime.UtcNow;
            var budgets = await _budgetRepo.GetByUserAndMonthAsync(userId, now.Year, now.Month);

            if (!budgets.Any()) return;

            var startDate = new DateTime(now.Year, now.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            var categoryTotals = await _expenseRepo.GetTotalByCategoryAsync(userId, startDate, endDate);

            foreach (var budget in budgets)
            {
                var spent = categoryTotals
                    .Where(kvp => kvp.Key.Equals(budget.Category, StringComparison.OrdinalIgnoreCase))
                    .Sum(kvp => kvp.Value);

                var percentageUsed = budget.MonthlyLimit > 0
                    ? (spent / budget.MonthlyLimit) * 100
                    : 0m;

                // Check 100% first (exceeded)
                if (percentageUsed >= 100)
                {
                    var alreadyNotified = await _notificationRepo.ExistsForBudgetThisMonthAsync(
                        userId, budget.Id, NotificationType.BudgetExceeded, now.Year, now.Month);

                    if (!alreadyNotified)
                    {
                        var notification = Notification.Create(
                            userId,
                            $"🚨 {budget.Category}: {spent:N2} / {budget.MonthlyLimit:N2} ({percentageUsed:N0}%)",
                            NotificationType.BudgetExceeded,
                            budget.Id
                        );
                        await _notificationRepo.AddAsync(notification);
                    }
                }
                // Check 80% (warning)
                else if (percentageUsed >= 80)
                {
                    var alreadyNotified = await _notificationRepo.ExistsForBudgetThisMonthAsync(
                        userId, budget.Id, NotificationType.BudgetWarning, now.Year, now.Month);

                    if (!alreadyNotified)
                    {
                        var notification = Notification.Create(
                            userId,
                            $"⚠️ {budget.Category}: {percentageUsed:N0}% ({spent:N2} / {budget.MonthlyLimit:N2})",
                            NotificationType.BudgetWarning,
                            budget.Id
                        );
                        await _notificationRepo.AddAsync(notification);
                    }
                }
            }
        }
    }
}
