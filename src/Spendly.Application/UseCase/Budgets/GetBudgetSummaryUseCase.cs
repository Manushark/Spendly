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

        public BudgetSummaryDto Execute(int userId, int year, int month)
        {
            var budgets = _budgetRepo.GetByUserAndMonth(userId, year, month);

            // Obtener todos los gastos del mes para calcular lo gastado por categoría
            var expenses = _expenseRepo.GetAll(userId, category: null, page: 1, pageSize: 10000)
                .Where(e => e.Date.Year == year && e.Date.Month == month)
                .ToList();

            var budgetDtos = budgets.Select(b =>
            {
                var spent = expenses
                    .Where(e => e.Category.Equals(b.Category, StringComparison.OrdinalIgnoreCase))
                    .Sum(e => e.Amount.Value);

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

