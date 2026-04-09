using Spendly.Application.DTOs.Budget;
using Spendly.Application.Interfaces;

namespace Spendly.Application.UseCases.Budgets
{
    public class GetBudgetSummaryUseCase
    {
        private readonly IBudgetRepository _budgetRepo;
        private readonly IExpenseRepository _expenseRepo;

        public GetBudgetSummaryUseCase(IBudgetRepository budgetRepo, IExpenseRepository expenseRepo)
        {
            _budgetRepo = budgetRepo;
            _expenseRepo = expenseRepo;
        }

        public async Task<BudgetSummaryDto> ExecuteAsync(int userId, int year, int month)
        {
            var budgets = await _budgetRepo.GetByUserAndMonthAsync(userId, year, month);

            // Filtrar gastos por fecha directamente en SQL en lugar de traer 10,000 registros
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            var categoryTotals = await _expenseRepo.GetTotalByCategoryAsync(userId, startDate, endDate);

            var budgetDtos = budgets.Select(b =>
            {
                var spent = categoryTotals
                    .Where(kvp => kvp.Key.Equals(b.Category, StringComparison.OrdinalIgnoreCase))
                    .Sum(kvp => kvp.Value);

                var remaining = b.MonthlyLimit - spent;
                var percentageUsed = b.MonthlyLimit > 0 ? (spent / b.MonthlyLimit) * 100 : 0;

                return new BudgetResponseDto
                {
                    Id = b.Id,
                    Category = b.Category,
                    MonthlyLimit = b.MonthlyLimit,
                    Year = b.Year,
                    Month = b.Month,
                    Spent = spent,
                    Remaining = remaining,
                    PercentageUsed = percentageUsed,
                    IsOverBudget = spent > b.MonthlyLimit,
                    CreatedAt = b.CreatedAt
                };
            }).ToList();

            return new BudgetSummaryDto
            {
                TotalBudgets = budgetDtos.Count,
                TotalLimit = budgetDtos.Sum(b => b.MonthlyLimit),
                TotalSpent = budgetDtos.Sum(b => b.Spent),
                TotalRemaining = budgetDtos.Sum(b => b.Remaining),
                BudgetsExceeded = budgetDtos.Count(b => b.IsOverBudget),
                Budgets = budgetDtos
            };
        }
    }
}
