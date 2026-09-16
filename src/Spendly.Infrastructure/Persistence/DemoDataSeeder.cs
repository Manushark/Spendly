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
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                await _categoryRepository.SeedDefaultsAsync(user.Id);
                await SeedFullDemoDataAsync(user.Id);
            }
            else
            {
                // Ensure the user has categories
                var categoryCount = await _context.Categories.CountAsync(c => c.UserId == user.Id);
                if (categoryCount == 0)
                {
                    await _categoryRepository.SeedDefaultsAsync(user.Id);
                }

                // If user has no expenses or budgets, seed fresh demo data
                var hasExpenses = await _context.Expenses.AnyAsync(e => e.UserId == user.Id);
                if (!hasExpenses)
                {
                    await SeedFullDemoDataAsync(user.Id);
                }
            }

            return user.Id;
        }

        public async Task ResetDemoDataAsync(int userId)
        {
            // Remove existing transactions and goals
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

            // 1. Incomes (Current Month)
            var income1 = Income.Create(userId, 4500.00m, "USD", "Tech Corp Salary", "Monthly direct deposit payroll", now.AddDays(-10), isRecurring: true);
            var income2 = Income.Create(userId, 950.00m, "USD", "Freelance UI/UX", "Mobile dashboard redesign sprint", now.AddDays(-3), isRecurring: false);
            _context.Incomes.AddRange(income1, income2);

            // 2. Budgets (Current Month)
            // Notice: Entertainment budget limit $250, with ~$220 spent = 88% to trigger domain Warning Alert!
            var budgetEntertainment = Budget.Create(userId, "Entertainment", 250.00m, currentYear, currentMonth);
            var budgetFood = Budget.Create(userId, "Food & Dining", 650.00m, currentYear, currentMonth);
            var budgetBills = Budget.Create(userId, "Bills & Utilities", 350.00m, currentYear, currentMonth);
            var budgetTransport = Budget.Create(userId, "Transportation", 200.00m, currentYear, currentMonth);
            var budgetShopping = Budget.Create(userId, "Shopping", 400.00m, currentYear, currentMonth);
            _context.Budgets.AddRange(budgetEntertainment, budgetFood, budgetBills, budgetTransport, budgetShopping);

            // 3. Current Month Expenses (Realistic spread across the month)
            var currentMonthExpenses = new List<Expense>
            {
                // Entertainment (Total: $220.00 -> 88% of $250.00 budget)
                Expense.Create(userId, Money.Create(75.00m, "USD"), "Concert VIP Tickets", now.AddDays(-8), "Entertainment"),
                Expense.Create(userId, Money.Create(45.00m, "USD"), "IMAX Cinema & Snacks", now.AddDays(-5), "Entertainment"),
                Expense.Create(userId, Money.Create(60.00m, "USD"), "Escape Room with Friends", now.AddDays(-2), "Entertainment"),
                Expense.Create(userId, Money.Create(40.00m, "USD"), "Steam Video Games Sale", now.AddDays(-1), "Entertainment"),

                // Food & Dining (Total: $325.30 -> 50% of $650.00 budget)
                Expense.Create(userId, Money.Create(135.50m, "USD"), "Whole Foods Market Groceries", now.AddDays(-9), "Food & Dining"),
                Expense.Create(userId, Money.Create(48.20m, "USD"), "Italian Bistro Dinner", now.AddDays(-7), "Food & Dining"),
                Expense.Create(userId, Money.Create(18.50m, "USD"), "Artisan Specialty Coffee & Bagels", now.AddDays(-4), "Food & Dining"),
                Expense.Create(userId, Money.Create(38.00m, "USD"), "Japanese Ramen Bar", now.AddDays(-3), "Food & Dining"),
                Expense.Create(userId, Money.Create(85.10m, "USD"), "Fresh Farm Weekly Produce", now.AddDays(-1), "Food & Dining"),

                // Bills & Utilities (Total: $175.00 -> 50% of $350.00 budget)
                Expense.Create(userId, Money.Create(110.00m, "USD"), "Gigabit Fiber Internet & Mobile Plan", now.AddDays(-10), "Bills & Utilities"),
                Expense.Create(userId, Money.Create(65.00m, "USD"), "Green Energy Electricity Bill", now.AddDays(-6), "Bills & Utilities"),

                // Transportation (Total: $72.50 -> 36% of $200.00 budget)
                Expense.Create(userId, Money.Create(48.00m, "USD"), "Full Gas Tank Refill", now.AddDays(-7), "Transportation"),
                Expense.Create(userId, Money.Create(24.50m, "USD"), "Uber Ride to Downtown Meeting", now.AddDays(-2), "Transportation"),

                // Shopping (Total: $139.98 -> 35% of $400.00 budget)
                Expense.Create(userId, Money.Create(94.99m, "USD"), "Mechanical Ergonomic Keyboard", now.AddDays(-5), "Shopping"),
                Expense.Create(userId, Money.Create(44.99m, "USD"), "Software Architecture Books", now.AddDays(-3), "Shopping")
            };
            _context.Expenses.AddRange(currentMonthExpenses);

            // 4. Previous Month Expenses (Gives depth to month-over-month charts and trendlines)
            var prevMonthDate = now.AddMonths(-1);
            var prevMonthDaysInMonth = DateTime.DaysInMonth(prevMonthDate.Year, prevMonthDate.Month);
            var prevMonthMid = new DateTime(prevMonthDate.Year, prevMonthDate.Month, Math.Min(15, prevMonthDaysInMonth));

            var previousMonthExpenses = new List<Expense>
            {
                Expense.Create(userId, Money.Create(280.00m, "USD"), "Monthly Supermarket Haul", prevMonthMid.AddDays(-5), "Food & Dining"),
                Expense.Create(userId, Money.Create(170.00m, "USD"), "Utility & Water Bills", prevMonthMid.AddDays(-3), "Bills & Utilities"),
                Expense.Create(userId, Money.Create(190.00m, "USD"), "Rock Festival Pass", prevMonthMid.AddDays(-1), "Entertainment"),
                Expense.Create(userId, Money.Create(65.00m, "USD"), "Highway Tolls & Fuel", prevMonthMid.AddDays(2), "Transportation"),
                Expense.Create(userId, Money.Create(120.00m, "USD"), "Noise-Cancelling Headphones", prevMonthMid.AddDays(4), "Shopping")
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
                SavingsGoal.Create(userId, "Emergency Fund (6 Months)", 10000.00m, 6500.00m, now.AddMonths(8), "bi-shield-check", "#10b981"),
                SavingsGoal.Create(userId, "Tech Conference & Trip to Tokyo", 3500.00m, 2800.00m, now.AddMonths(4), "bi-airplane", "#6366f1"),
                SavingsGoal.Create(userId, "MacBook Pro M3 Max", 2800.00m, 1400.00m, now.AddMonths(6), "bi-laptop", "#f59e0b")
            };
            _context.SavingsGoals.AddRange(savingsGoals);

            await _context.SaveChangesAsync();
        }
    }
}
