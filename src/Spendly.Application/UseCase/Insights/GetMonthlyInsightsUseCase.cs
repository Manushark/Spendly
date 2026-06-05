using Spendly.Application.DTOs.Insights;
using Spendly.Application.Interfaces;

namespace Spendly.Application.UseCases.Insights
{
    public class GetMonthlyInsightsUseCase
    {
        private readonly IExpenseRepository _expenseRepo;
        private readonly IIncomeRepository _incomeRepo;
        private readonly IDateTimeProvider _dateTime;

        public GetMonthlyInsightsUseCase(IExpenseRepository expenseRepo, IIncomeRepository incomeRepo, IDateTimeProvider dateTime)
        {
            _expenseRepo = expenseRepo;
            _incomeRepo = incomeRepo;
            _dateTime = dateTime;
        }

        public async Task<MonthlyInsightsDto> ExecuteAsync(int userId, int year, int month, string? userTimeZone = null)
        {
            // Current month range
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var today = _dateTime.Now(userTimeZone);
            var daysElapsed = today.Year == year && today.Month == month
                ? today.Day
                : daysInMonth;

            // Previous month range
            var prevStart = startDate.AddMonths(-1);
            var prevEnd = startDate.AddDays(-1);

            // ── Fetch data ──
            var currentExpenses = (await _expenseRepo.GetByDateRangeAsync(userId, startDate, endDate)).ToList();
            var prevExpenses = (await _expenseRepo.GetByDateRangeAsync(userId, prevStart, prevEnd)).ToList();

            var totalExpenses = currentExpenses.Sum(e => e.Amount.Value);
            var prevTotalExpenses = prevExpenses.Sum(e => e.Amount.Value);
            var totalIncome = await _incomeRepo.GetTotalAmountAsync(userId, startDate, endDate);
            var prevTotalIncome = await _incomeRepo.GetTotalAmountAsync(userId, prevStart, prevEnd);

            var balance = totalIncome - totalExpenses;
            var savingsRate = totalIncome > 0 ? ((totalIncome - totalExpenses) / totalIncome) * 100 : 0;

            // ── Category Breakdown ──
            var prevCategoryTotals = prevExpenses
                .GroupBy(e => e.Category)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount.Value), StringComparer.OrdinalIgnoreCase);

            var categoryBreakdown = currentExpenses
                .GroupBy(e => e.Category)
                .Select(g =>
                {
                    var amount = g.Sum(e => e.Amount.Value);
                    var prevAmount = prevCategoryTotals.GetValueOrDefault(g.Key, 0m);
                    return new CategoryInsightDto
                    {
                        Category = g.Key,
                        Amount = amount,
                        Percentage = totalExpenses > 0 ? (amount / totalExpenses) * 100 : 0,
                        TransactionCount = g.Count(),
                        PreviousMonthAmount = prevAmount,
                        ChangePercent = prevAmount > 0 ? ((amount - prevAmount) / prevAmount) * 100 : (amount > 0 ? 100 : 0)
                    };
                })
                .OrderByDescending(c => c.Amount)
                .ToList();

            // ── Daily Spending ──
            var dailySpending = Enumerable.Range(0, daysInMonth)
                .Select(d =>
                {
                    var date = startDate.AddDays(d);
                    var dayAmount = currentExpenses
                        .Where(e => e.Date.Date == date.Date)
                        .Sum(e => e.Amount.Value);
                    return new DailySpendingDto { Date = date, Amount = dayAmount };
                })
                .ToList();

            // ── Weekday Analysis ──
            var weekdayGroups = currentExpenses
                .GroupBy(e => e.Date.DayOfWeek)
                .Select(g =>
                {
                    var dayCount = Enumerable.Range(0, daysInMonth)
                        .Count(d => startDate.AddDays(d).DayOfWeek == g.Key);
                    var totalSpent = g.Sum(e => e.Amount.Value);
                    return new WeekdayInsightDto
                    {
                        DayName = g.Key.ToString(),
                        DayOfWeek = (int)g.Key,
                        TotalSpent = totalSpent,
                        Average = dayCount > 0 ? totalSpent / dayCount : 0,
                        TransactionCount = g.Count()
                    };
                })
                .OrderBy(w => w.DayOfWeek)
                .ToList();

            // Fill missing days
            for (int i = 0; i < 7; i++)
            {
                if (!weekdayGroups.Any(w => w.DayOfWeek == i))
                {
                    weekdayGroups.Add(new WeekdayInsightDto
                    {
                        DayName = ((DayOfWeek)i).ToString(),
                        DayOfWeek = i,
                        TotalSpent = 0,
                        Average = 0,
                        TransactionCount = 0
                    });
                }
            }
            weekdayGroups = weekdayGroups.OrderBy(w => w.DayOfWeek).ToList();

            // ── Projection ──
            var dailyAverage = daysElapsed > 0 ? totalExpenses / daysElapsed : 0;
            var projected = dailyAverage * daysInMonth;

            return new MonthlyInsightsDto
            {
                Year = year,
                Month = month,
                MonthName = startDate.ToString("MMMM yyyy"),
                TotalExpenses = totalExpenses,
                TotalIncome = totalIncome,
                Balance = balance,
                SavingsRate = savingsRate,
                PreviousMonthExpenses = prevTotalExpenses,
                ExpenseChangePercent = prevTotalExpenses > 0 ? ((totalExpenses - prevTotalExpenses) / prevTotalExpenses) * 100 : 0,
                PreviousMonthIncome = prevTotalIncome,
                IncomeChangePercent = prevTotalIncome > 0 ? ((totalIncome - prevTotalIncome) / prevTotalIncome) * 100 : 0,
                CategoryBreakdown = categoryBreakdown,
                DailySpending = dailySpending,
                WeekdayAnalysis = weekdayGroups,
                ProjectedMonthEnd = projected,
                DaysElapsed = daysElapsed,
                DaysInMonth = daysInMonth,
                DailyAverage = dailyAverage
            };
        }
    }
}
