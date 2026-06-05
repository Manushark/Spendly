using Spendly.Application.Interfaces;
using Spendly.Domain.Entities;
using Spendly.Domain.Enums;

namespace Spendly.Application.Services
{
    /// <summary>
    /// Servicio que verifica presupuestos después de crear o editar gastos
    /// y genera notificaciones cuando se alcanza el 80% o 100%.
    /// </summary>
    public class BudgetAlertService
    {
        private readonly IBudgetRepository _budgetRepo;
        private readonly IExpenseRepository _expenseRepo;
        private readonly INotificationRepository _notificationRepo;
        private readonly IUserRepository _userRepo;
        private readonly IDateTimeProvider _dateTime;

        public BudgetAlertService(
            IBudgetRepository budgetRepo,
            IExpenseRepository expenseRepo,
            INotificationRepository notificationRepo,
            IUserRepository userRepo,
            IDateTimeProvider dateTime)
        {
            _budgetRepo = budgetRepo;
            _expenseRepo = expenseRepo;
            _notificationRepo = notificationRepo;
            _userRepo = userRepo;
            _dateTime = dateTime;
        }

        public async Task CheckAndCreateAlertsAsync(int userId)
        {
            // Load user timezone to determine the correct local month
            var user = await _userRepo.GetByIdAsync(userId);
            var now = _dateTime.Now(user?.TimeZone);
            var budgets = await _budgetRepo.GetByUserAndMonthAsync(userId, now.Year, now.Month);

            Console.WriteLine($"[BudgetAlert] userId={userId} month={now.Year}/{now.Month} budgets found={budgets.Count}");

            if (!budgets.Any()) return;

            // Use date-only comparisons (no time component) to match how Expense.Date is stored
            var startDate = new DateTime(now.Year, now.Month, 1);
            var endDate   = startDate.AddMonths(1).AddDays(-1); // last day of month, inclusive (<=)

            var categoryTotals = await _expenseRepo.GetTotalByCategoryAsync(userId, startDate, endDate);

            Console.WriteLine($"[BudgetAlert] categoryTotals={string.Join(", ", categoryTotals.Select(k => $"{k.Key}:{k.Value:N2}"))}");

            foreach (var budget in budgets)
            {
                var spent = categoryTotals
                    .Where(kvp => kvp.Key.Equals(budget.Category, StringComparison.OrdinalIgnoreCase))
                    .Sum(kvp => kvp.Value);

                var percentageUsed = budget.MonthlyLimit > 0
                    ? (spent / budget.MonthlyLimit) * 100
                    : 0m;

                Console.WriteLine($"[BudgetAlert] category={budget.Category} limit={budget.MonthlyLimit:N2} spent={spent:N2} pct={percentageUsed:N0}%");

                if (percentageUsed >= 100)
                {
                    var alreadyExceeded = await _notificationRepo.ExistsForBudgetThisMonthAsync(
                        userId, budget.Id, NotificationType.BudgetExceeded, now.Year, now.Month);

                    Console.WriteLine($"[BudgetAlert] EXCEEDED — alreadyExceeded={alreadyExceeded}");

                    if (!alreadyExceeded)
                    {
                        var notification = Notification.Create(
                            userId,
                            $"\U0001f6a8 {budget.Category}: {spent:N2} / {budget.MonthlyLimit:N2} ({percentageUsed:N0}%)",
                            NotificationType.BudgetExceeded,
                            budget.Id
                        );
                        await _notificationRepo.AddAsync(notification);
                        Console.WriteLine($"[BudgetAlert] Created BudgetExceeded notification.");
                    }
                }
                else if (percentageUsed >= 80)
                {
                    var alreadyWarned = await _notificationRepo.ExistsForBudgetThisMonthAsync(
                        userId, budget.Id, NotificationType.BudgetWarning, now.Year, now.Month);
                    var alreadyExceeded = await _notificationRepo.ExistsForBudgetThisMonthAsync(
                        userId, budget.Id, NotificationType.BudgetExceeded, now.Year, now.Month);

                    Console.WriteLine($"[BudgetAlert] WARNING — alreadyWarned={alreadyWarned} alreadyExceeded={alreadyExceeded}");

                    if (!alreadyWarned && !alreadyExceeded)
                    {
                        var notification = Notification.Create(
                            userId,
                            $"\u26a0\ufe0f {budget.Category}: {percentageUsed:N0}% ({spent:N2} / {budget.MonthlyLimit:N2})",
                            NotificationType.BudgetWarning,
                            budget.Id
                        );
                        await _notificationRepo.AddAsync(notification);
                        Console.WriteLine($"[BudgetAlert] Created BudgetWarning notification.");
                    }
                }
                else
                {
                    Console.WriteLine($"[BudgetAlert] Below threshold, no notification.");
                }
            }
        }
    }
}
