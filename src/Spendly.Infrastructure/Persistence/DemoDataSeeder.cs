using Microsoft.EntityFrameworkCore;
using Spendly.Application.Interfaces;
using Spendly.Domain.Entities;
using Spendly.Domain.Enums;
using Spendly.Domain.ValueObjects;

namespace Spendly.Infrastructure.Persistence
{
    public class DemoDataSeeder : IDemoDataSeeder
    {
        private const string DemoEmail = "demo@spendly.com";
        private readonly SpendlyDbContext _context;
        private readonly ICategoryRepository _categoryRepository;

        public DemoDataSeeder(SpendlyDbContext context, ICategoryRepository categoryRepository)
        {
            _context = context;
            _categoryRepository = categoryRepository;
        }

        public async Task<int> EnsureDemoUserAndDataAsync()
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == DemoEmail);

            if (user == null)
            {
                var passwordHash = BCrypt.Net.BCrypt.HashPassword("DemoPassword123!");
                user = User.Create(DemoEmail, passwordHash);
                user.UpdateProfile("Recruiter Demo User", "USD", "America/New_York");
                user.UpdateNotificationPreferences(false, false, false);
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                await _categoryRepository.SeedDefaultsAsync(user.Id);
                await SeedFullDemoDataAsync(user.Id);
            }
            else
            {
                // Ensure notification preferences are off for demo user to avoid bounce emails
                if (user.EmailNotificationsEnabled || user.BudgetAlertEmailEnabled || user.WeeklySummaryEmailEnabled)
                {
                    user.UpdateNotificationPreferences(false, false, false);
                    await _context.SaveChangesAsync();
                }

                // Ensure the user has categories
                var categoryCount = await _context.Categories.CountAsync(c => c.UserId == user.Id);
                if (categoryCount == 0)
                {
                    await _categoryRepository.SeedDefaultsAsync(user.Id);
                }

                var now = DateTime.UtcNow.Date;

                // Check if demo user has expenses in the CURRENT calendar month and year
                var hasCurrentMonthExpenses = await _context.Expenses
                    .AnyAsync(e => e.UserId == user.Id && e.Date.Year == now.Year && e.Date.Month == now.Month);

                // Also check if current month budgets exist
                var hasCurrentMonthBudgets = await _context.Budgets
                    .AnyAsync(b => b.UserId == user.Id && b.Year == now.Year && b.Month == now.Month);

                if (!hasCurrentMonthExpenses || !hasCurrentMonthBudgets)
                {
                    // If demo data is from a past month, incomplete, or absent,
                    // automatically wipe stale transactions and seed fresh, randomized data for the active month!
                    await ResetDemoDataAsync(user.Id);
                }
            }

            return user.Id;
        }

        public async Task ResetDemoDataAsync(int userId)
        {
            // Remove existing transactions, notifications, and goals
            var existingExpenses = await _context.Expenses.IgnoreQueryFilters().Where(e => e.UserId == userId).ToListAsync();
            _context.Expenses.RemoveRange(existingExpenses);

            var existingBudgets = await _context.Budgets.Where(b => b.UserId == userId).ToListAsync();
            _context.Budgets.RemoveRange(existingBudgets);

            var existingIncomes = await _context.Incomes.Where(i => i.UserId == userId).ToListAsync();
            _context.Incomes.RemoveRange(existingIncomes);

            var existingRecurring = await _context.RecurringExpenses.Where(r => r.UserId == userId).ToListAsync();
            _context.RecurringExpenses.RemoveRange(existingRecurring);

            var existingGoals = await _context.SavingsGoals.Where(s => s.UserId == userId).ToListAsync();
            _context.SavingsGoals.RemoveRange(existingGoals);

            var existingNotifications = await _context.Notifications.Where(n => n.UserId == userId).ToListAsync();
            _context.Notifications.RemoveRange(existingNotifications);

            // Ensure notification preferences remain disabled for demo user
            var demoUser = await _context.Users.FindAsync(userId);
            if (demoUser != null && (demoUser.EmailNotificationsEnabled || demoUser.BudgetAlertEmailEnabled || demoUser.WeeklySummaryEmailEnabled))
            {
                demoUser.UpdateNotificationPreferences(false, false, false);
            }

            await _context.SaveChangesAsync();

            // Re-seed default categories if missing
            var categoryCount = await _context.Categories.CountAsync(c => c.UserId == userId);
            if (categoryCount == 0)
            {
                await _categoryRepository.SeedDefaultsAsync(userId);
            }

            // Populate fresh, rich financial dataset
            await SeedFullDemoDataAsync(userId);
        }

        private async Task SeedFullDemoDataAsync(int userId)
        {
            var now = DateTime.UtcNow.Date;
            var currentYear = now.Year;
            var currentMonth = now.Month;
            var currentDay = now.Day;

            var rng = new Random();

            // Helper to compute a date strictly within current month (clamped between 1 and currentDay)
            DateTime CurrentMonthDate(int dayOffset, int hour = 12)
            {
                var targetDay = Math.Clamp(currentDay - dayOffset, 1, currentDay);
                return new DateTime(currentYear, currentMonth, targetDay, hour, 0, 0, DateTimeKind.Utc);
            }

            // Helper to apply realistic randomized jitter around a base amount
            decimal Jitter(decimal baseAmount, double minMultiplier = 0.93, double maxMultiplier = 1.07)
            {
                var factor = (decimal)(minMultiplier + (rng.NextDouble() * (maxMultiplier - minMultiplier)));
                return Math.Round(baseAmount * factor, 2);
            }

            // 1. Incomes (Current Month)
            // Predictable primary salary on 1st of month + realistic freelance project income
            var salaryDate = new DateTime(currentYear, currentMonth, 1, 9, 0, 0, DateTimeKind.Utc);
            var income1 = Income.Create(userId, 4500.00m, "USD", "Tech Corp Salary", "Monthly direct deposit payroll", salaryDate, isRecurring: true);
            var freelanceAmount = Jitter(950.00m, 0.90, 1.15);
            var income2 = Income.Create(userId, freelanceAmount, "USD", "Freelance UI/UX", "Mobile dashboard redesign sprint", CurrentMonthDate(3, 15), isRecurring: false);
            _context.Incomes.AddRange(income1, income2);

            // 2. Budgets (Current Month)
            // Notice: Entertainment budget limit $250.00 with ~$210-$230 spent (84%-92%) to trigger domain Warning Alert!
            var budgetEntertainment = Budget.Create(userId, "Entertainment", 250.00m, currentYear, currentMonth);
            var budgetFood = Budget.Create(userId, "Food & Dining", 650.00m, currentYear, currentMonth);
            var budgetBills = Budget.Create(userId, "Bills & Utilities", 350.00m, currentYear, currentMonth);
            var budgetTransport = Budget.Create(userId, "Transportation", 200.00m, currentYear, currentMonth);
            var budgetShopping = Budget.Create(userId, "Shopping", 400.00m, currentYear, currentMonth);
            _context.Budgets.AddRange(budgetEntertainment, budgetFood, budgetBills, budgetTransport, budgetShopping);

            // Previous month budgets for rich historical comparison
            var prevMonthDate = now.AddMonths(-1);
            var prevMonthYear = prevMonthDate.Year;
            var prevMonthMonth = prevMonthDate.Month;
            var prevBudgetEntertainment = Budget.Create(userId, "Entertainment", 250.00m, prevMonthYear, prevMonthMonth);
            var prevBudgetFood = Budget.Create(userId, "Food & Dining", 650.00m, prevMonthYear, prevMonthMonth);
            var prevBudgetBills = Budget.Create(userId, "Bills & Utilities", 350.00m, prevMonthYear, prevMonthMonth);
            var prevBudgetTransport = Budget.Create(userId, "Transportation", 200.00m, prevMonthYear, prevMonthMonth);
            var prevBudgetShopping = Budget.Create(userId, "Shopping", 400.00m, prevMonthYear, prevMonthMonth);
            _context.Budgets.AddRange(prevBudgetEntertainment, prevBudgetFood, prevBudgetBills, prevBudgetTransport, prevBudgetShopping);

            // 3. Current Month Expenses (Realistic spread across the month up to today)
            var currentMonthExpenses = new List<Expense>
            {
                // Entertainment (Controlled jitter: sum is mathematically guaranteed between $205 and $235, i.e., 82% - 94% of $250 limit)
                Expense.Create(userId, Money.Create(Jitter(75.00m, 0.94, 1.06), "USD"), "Concert VIP Tickets", CurrentMonthDate(7, 20), "Entertainment"),
                Expense.Create(userId, Money.Create(Jitter(45.00m, 0.94, 1.06), "USD"), "IMAX Cinema & Snacks", CurrentMonthDate(4, 18), "Entertainment"),
                Expense.Create(userId, Money.Create(Jitter(60.00m, 0.94, 1.06), "USD"), "Escape Room with Friends", CurrentMonthDate(2, 19), "Entertainment"),
                Expense.Create(userId, Money.Create(Jitter(40.00m, 0.94, 1.06), "USD"), "Steam Video Games Sale", CurrentMonthDate(0, 21), "Entertainment"),

                // Food & Dining (Total: ~$300-$350 -> ~50% of $650 budget, healthy and green)
                Expense.Create(userId, Money.Create(Jitter(135.50m, 0.92, 1.08), "USD"), "Whole Foods Market Groceries", CurrentMonthDate(8, 11), "Food & Dining"),
                Expense.Create(userId, Money.Create(Jitter(48.20m, 0.92, 1.08), "USD"), "Italian Bistro Dinner", CurrentMonthDate(6, 20), "Food & Dining"),
                Expense.Create(userId, Money.Create(Jitter(18.50m, 0.90, 1.10), "USD"), "Artisan Specialty Coffee & Bagels", CurrentMonthDate(3, 8), "Food & Dining"),
                Expense.Create(userId, Money.Create(Jitter(38.00m, 0.92, 1.08), "USD"), "Japanese Ramen Bar", CurrentMonthDate(2, 13), "Food & Dining"),
                Expense.Create(userId, Money.Create(Jitter(85.10m, 0.92, 1.08), "USD"), "Fresh Farm Weekly Produce", CurrentMonthDate(0, 10), "Food & Dining"),

                // Bills & Utilities (Total: ~$165-$185 -> ~50% of $350 budget)
                Expense.Create(userId, Money.Create(Jitter(110.00m, 0.95, 1.05), "USD"), "Gigabit Fiber Internet & Mobile Plan", CurrentMonthDate(9, 14), "Bills & Utilities"),
                Expense.Create(userId, Money.Create(Jitter(65.00m, 0.90, 1.10), "USD"), "Green Energy Electricity Bill", CurrentMonthDate(5, 16), "Bills & Utilities"),

                // Transportation (Total: ~$68-$80 -> ~36% of $200 budget)
                Expense.Create(userId, Money.Create(Jitter(48.00m, 0.92, 1.08), "USD"), "Full Gas Tank Refill", CurrentMonthDate(6, 17), "Transportation"),
                Expense.Create(userId, Money.Create(Jitter(24.50m, 0.90, 1.15), "USD"), "Uber Ride to Downtown Meeting", CurrentMonthDate(1, 9), "Transportation"),

                // Shopping (Total: ~$130-$150 -> ~35% of $400 budget)
                Expense.Create(userId, Money.Create(Jitter(94.99m, 0.92, 1.08), "USD"), "Mechanical Ergonomic Keyboard", CurrentMonthDate(4, 15), "Shopping"),
                Expense.Create(userId, Money.Create(Jitter(44.99m, 0.90, 1.10), "USD"), "Software Architecture Books", CurrentMonthDate(2, 12), "Shopping")
            };
            _context.Expenses.AddRange(currentMonthExpenses);

            // 4. Previous Month Expenses (Gives depth to month-over-month charts and trendlines)
            var prevMonthDaysInMonth = DateTime.DaysInMonth(prevMonthYear, prevMonthMonth);
            DateTime PrevMonthDate(int day, int hour = 14) =>
                new DateTime(prevMonthYear, prevMonthMonth, Math.Clamp(day, 1, prevMonthDaysInMonth), hour, 0, 0, DateTimeKind.Utc);

            var previousMonthExpenses = new List<Expense>
            {
                Expense.Create(userId, Money.Create(Jitter(280.00m, 0.92, 1.08), "USD"), "Monthly Supermarket Haul", PrevMonthDate(5), "Food & Dining"),
                Expense.Create(userId, Money.Create(Jitter(170.00m, 0.95, 1.05), "USD"), "Utility & Water Bills", PrevMonthDate(10), "Bills & Utilities"),
                Expense.Create(userId, Money.Create(Jitter(190.00m, 0.92, 1.08), "USD"), "Rock Festival Pass", PrevMonthDate(15), "Entertainment"),
                Expense.Create(userId, Money.Create(Jitter(65.00m, 0.90, 1.10), "USD"), "Highway Tolls & Fuel", PrevMonthDate(20), "Transportation"),
                Expense.Create(userId, Money.Create(Jitter(120.00m, 0.92, 1.08), "USD"), "Noise-Cancelling Headphones", PrevMonthDate(25), "Shopping")
            };
            _context.Expenses.AddRange(previousMonthExpenses);

            // 5. Recurring Expenses (Subscriptions & Recurring commitments)
            var recurring = new List<RecurringExpense>
            {
                RecurringExpense.Create(userId, "Netflix Premium 4K", 22.99m, "Entertainment", RecurrenceFrequency.Monthly, now.AddMonths(-4)),
                RecurringExpense.Create(userId, "Spotify Family Subscription", 16.99m, "Entertainment", RecurrenceFrequency.Monthly, now.AddMonths(-3)),
                RecurringExpense.Create(userId, "Modern Apartment Lease", 1250.00m, "Bills & Utilities", RecurrenceFrequency.Monthly, now.AddMonths(-6)),
                RecurringExpense.Create(userId, "Crossfit & Gym Pass", 45.00m, "Health", RecurrenceFrequency.Monthly, now.AddMonths(-2))
            };
            _context.RecurringExpenses.AddRange(recurring);

            // 6. Savings Goals (Milestones with visual progress bars)
            var savingsGoals = new List<SavingsGoal>
            {
                SavingsGoal.Create(userId, "Emergency Fund (6 Months)", 10000.00m, Jitter(6500.00m, 0.96, 1.04), now.AddMonths(8), "bi-shield-check", "#10b981"),
                SavingsGoal.Create(userId, "Tech Conference & Trip to Tokyo", 3500.00m, Jitter(2800.00m, 0.96, 1.04), now.AddMonths(4), "bi-airplane", "#6366f1"),
                SavingsGoal.Create(userId, "MacBook Pro M3 Max", 2800.00m, Jitter(1400.00m, 0.96, 1.04), now.AddMonths(6), "bi-laptop", "#f59e0b")
            };
            _context.SavingsGoals.AddRange(savingsGoals);

            // 7. In-App Notifications (Showcases alert badge and notification tray)
            var entertainmentSpent = currentMonthExpenses.Where(e => e.Category == "Entertainment").Sum(e => e.Amount.Value);
            var entertainmentPct = (int)Math.Round((entertainmentSpent / budgetEntertainment.MonthlyLimit) * 100);

            var notifications = new List<Notification>
            {
                Notification.Create(
                    userId,
                    $"Budget Warning: You have reached {entertainmentPct}% of your Entertainment budget for {now:MMMM} (${entertainmentSpent:N2} of ${budgetEntertainment.MonthlyLimit:N2}).",
                    NotificationType.BudgetWarning),
                Notification.Create(
                    userId,
                    "Savings Milestone: 'Tech Conference & Trip to Tokyo' goal reached 80% completion!",
                    NotificationType.SavingsGoalMilestone)
            };
            _context.Notifications.AddRange(notifications);

            await _context.SaveChangesAsync();
        }
    }
}
