using Spendly.Application.DTOs.Budget;
using Spendly.Application.Interfaces;
using Spendly.Domain.Exceptions;

namespace Spendly.Application.UseCases.Budgets
{
    public class UpdateBudgetUseCase
    {
        private readonly IBudgetRepository _repo;

        public UpdateBudgetUseCase(IBudgetRepository repo) => _repo = repo;

        public async Task ExecuteAsync(int userId, int id, UpdateBudgetDto dto)
        {
            var budget = await _repo.GetByIdAsync(id)
                ?? throw new InvalidDomainException($"Budget {id} not found.");

            budget.EnsureOwnership(userId);
            budget.Update(dto.Category, dto.MonthlyLimit, dto.Year, dto.Month);
            await _repo.UpdateAsync(budget);
        }
    }

    public class DeleteBudgetUseCase
    {
        private readonly IBudgetRepository _repo;

        public DeleteBudgetUseCase(IBudgetRepository repo) => _repo = repo;

        public async Task<bool> ExecuteAsync(int userId, int id)
        {
            var budget = await _repo.GetByIdAsync(id);
            if (budget == null) return false;

            budget.EnsureOwnership(userId);
            return await _repo.DeleteAsync(id);
        }
    }

    public class GetBudgetByIdUseCase
    {
        private readonly IBudgetRepository _budgetRepo;
        private readonly IExpenseRepository _expenseRepo;

        public GetBudgetByIdUseCase(IBudgetRepository budgetRepo, IExpenseRepository expenseRepo)
        {
            _budgetRepo = budgetRepo;
            _expenseRepo = expenseRepo;
        }

        public async Task<BudgetResponseDto?> ExecuteAsync(int userId, int id)
        {
            var budget = await _budgetRepo.GetByIdAsync(id);
            if (budget == null) return null;

            budget.EnsureOwnership(userId);

            // Filtrar directamente en SQL en lugar de traer 10,000 registros
            var startDate = new DateTime(budget.Year, budget.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            var categoryTotals = await _expenseRepo.GetTotalByCategoryAsync(userId, startDate, endDate);
            var spent = categoryTotals
                .Where(kvp => kvp.Key.Equals(budget.Category, StringComparison.OrdinalIgnoreCase))
                .Sum(kvp => kvp.Value);

            var remaining = budget.MonthlyLimit - spent;
            var percentageUsed = budget.MonthlyLimit > 0 ? (spent / budget.MonthlyLimit) * 100 : 0;

            return new BudgetResponseDto
            {
                Id = budget.Id,
                Category = budget.Category,
                MonthlyLimit = budget.MonthlyLimit,
                Year = budget.Year,
                Month = budget.Month,
                Spent = spent,
                Remaining = remaining,
                PercentageUsed = percentageUsed,
                IsOverBudget = spent > budget.MonthlyLimit,
                CreatedAt = budget.CreatedAt
            };
        }
    }
}
